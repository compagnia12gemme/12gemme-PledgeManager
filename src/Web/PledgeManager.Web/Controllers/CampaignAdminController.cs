using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.Models;
using PledgeManager.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PledgeManager.Web.Controllers {

    [Authorize(Policy = Startup.CampaignLoginPolicy)]
    public class CampaignAdminController : Controller {

        private readonly MongoDatabase _database;
        private readonly MailComposer _composer;
        private readonly ILogger<CampaignAdminController> _logger;

        private const string TempKeyNotification = nameof(TempKeyNotification);

        public CampaignAdminController(
            MongoDatabase database,
            MailComposer composer,
            ILogger<CampaignAdminController> logger
        ) {
            _database = database;
            _composer = composer;
            _logger = logger;
        }

        private IActionResult RedirectToIndexWithNotification(
            string campaignCode,
            bool isError,
            string notification
        ) {
            this.AddToTemp(TempKeyNotification, new PerformedAction {
                IsError = isError,
                Message = notification
            });
            return RedirectToAction(nameof(Index), "CampaignAdmin", new {
                campaignCode
            }, "notification");
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromRoute] string campaignCode
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var rewardMap = campaign.Rewards.ToDictionary(reward => reward.Code);

            (var pledges, var closedCount) = await _database.GetPledges(campaign.Id);

            var vm = new CampaignDashboardViewModel {
                Campaign = campaign,
                Pledges = from p in pledges
                          select (
                              p,
                              rewardMap[p.OriginalRewardLevel],
                              rewardMap[p.CurrentRewardLevel]
                          ),
                PledgeCount = pledges.Count,
                ClosedPledgeCount = (int)closedCount
            };
            vm.Notification = this.FromTemp<PerformedAction>(TempKeyNotification);

            return View("Home", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite(
            [FromRoute] string campaignCode,
            [FromForm] int userId
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var pledge = await _database.GetPledge(campaign.Id, userId);

            _composer.SendInvitation(campaign, pledge);

            return RedirectToIndexWithNotification(campaignCode, false,
                $"Invitation email scheduled for pledge #{userId}.");
        }

        [HttpPost]
        public async Task<IActionResult> SendReminder(
            [FromRoute] string campaignCode,
            [FromForm] int userId
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var pledge = await _database.GetPledge(campaign.Id, userId);

            _composer.SendReminder(campaign, pledge);

            return RedirectToIndexWithNotification(campaignCode, false,
                $"Reminder email scheduled for pledge #{userId}.");
        }

        public async Task<IActionResult> ForceClose(
            [FromRoute] string campaignCode,
            [FromForm] int userId
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var pledge = await _database.GetPledge(campaign.Id, userId);



            return RedirectToIndexWithNotification(campaignCode, false,
                $"Pledge #{userId} closed.");
        }

        private static readonly Dictionary<string, string> CountryMap = new Dictionary<string, string>() {
            { "IT", "Italia" },
            { "DE", "Germania" }
        };

        [HttpPost]
        public async Task<IActionResult> ImportKickstarterBackers(
            [FromRoute] string campaignCode,
            IFormFile backerFile
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var rewardImportMap = campaign.Rewards.ToDictionary(r => r.ImportRewardId);

            _logger.LogInformation("Importing backers for campaign {0}", campaignCode);

            using var source = backerFile.OpenReadStream();
            using var streamReader = new StreamReader(source);
            using var reader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            var records = reader.GetRecordsAsync<KickstarterBackerInfo>();
            int count = 0;
            var rnd = new Random();
            await foreach(var record in records) {
                ++count;

                if(record.RewardID == default) {
                    _logger.LogInformation("Skipping record {0} {1} with no reward", record.BackerNumber, record.Email);
                    continue;
                }

                var names = record.BackerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if(!record.PledgeAmount.StartsWith('€')) {
                    _logger.LogError("Record pledge does not start with euro symbol: {0}", record.PledgeAmount);
                    return BadRequest();
                }
                if(!decimal.TryParse(record.PledgeAmount.Substring(1), NumberStyles.Currency, CultureInfo.InvariantCulture, out var pledgeAmount)) {
                    _logger.LogError("Cannot parse pledge amount: {0}", record.PledgeAmount);
                    return BadRequest();
                }
                if (!decimal.TryParse(record.ShippingAmount.Substring(1), NumberStyles.Currency, CultureInfo.InvariantCulture, out var shippingAmount)) {
                    _logger.LogError("Cannot parse shipping amount: {0}", record.ShippingAmount);
                    return BadRequest();
                }

                var pledge = new Pledge {
                    CampaignId = campaign.Id,
                    UserId = record.BackerNumber,
                    UserToken = rnd.GenerateCode(8),
                    Email = record.Email,
                    Shipping = new ShippingInfo {
                        GivenName = string.Join(' ', names.Take(names.Length - 1)),
                        Surname = names[^1],
                        Country = CountryMap.ContainsKey(record.ShippingCountry) ? CountryMap[record.ShippingCountry] : record.ShippingCountry
                    },
                    OriginalPledge = pledgeAmount,
                    OriginalRewardLevel = rewardImportMap[record.RewardID].Code,
                    OriginalShippingPayment = shippingAmount
                };
                await _database.InsertPledge(pledge);

                _logger.LogDebug("Backer {0} {1} registered", record.BackerNumber, record.BackerName);
            }

            _logger.LogInformation("Processed {0} backers", count);

            return RedirectToIndexWithNotification(campaignCode, false, "Done");
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitations(
            [FromRoute] string campaignCode
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            (var pledges, _) = await _database.GetPledges(campaign.Id);

            int count = 0;
            foreach (var pledge in pledges) {
                count++;
                _composer.SendInvitation(campaign, pledge);
            }

            return RedirectToIndexWithNotification(campaignCode, false,
                $"Scheduled {count} invitation emails.");
        }

        [HttpPost]
        public async Task<IActionResult> ExportCsv(
            [FromRoute] string campaignCode
        ) {
            var campaign = await _database.GetCampaign(campaignCode);
            var pledges = await _database.GetClosedPledges(campaign.Id);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms, Encoding.UTF8);

            writer.Write("UserID,Email,Name,Surname,Address,City,ZipCode,Province,Country,FinalPledge,");
            foreach(var r in campaign.Rewards) {
                writer.Write("{0},", r.Code);
            }
            foreach(var a in campaign.AddOns) {
                writer.Write("{0},", a.Code);
            }
            foreach(var s in campaign.Survey) {
                writer.Write("{0},", s.Name);
            }
            writer.Write("Note,");
            writer.WriteLine();

            foreach(var p in pledges) {
                writer.Write("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",",
                    p.UserId,
                    p.Email,
                    p.Shipping?.GivenName,
                    p.Shipping?.Surname,
                    p.Shipping?.Address + ((p.Shipping?.AddressSecondary != null) ? (", " + p.Shipping.AddressSecondary) : string.Empty),
                    p.Shipping?.City,
                    p.Shipping?.ZipCode,
                    p.Shipping?.Province,
                    p.Shipping?.Country,
                    p.CurrentPledge.ToString(CultureInfo.InvariantCulture)
                );
                foreach(var r in campaign.Rewards) {
                    writer.Write("{0},",
                        (r.Code == p.CurrentRewardLevel) ? "1" : "0"
                    );
                }
                foreach (var a in campaign.AddOns) {
                    writer.Write("{0},",
                        p.AddOns.Count(addon => addon.Code == a.Code)
                    );
                }
                foreach (var s in campaign.Survey) {
                    writer.Write("{0},",
                        p.Survey.ReadSurveyValue(s)
                    );
                }
                writer.Write("\"{0}\",", Regex.Replace(p.Note ?? string.Empty, "[\n\r]*", string.Empty));
                writer.WriteLine();
            }

            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "text/csv",
                string.Format("backers.csv", campaign.Code, DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
            );
        }

    }

}
