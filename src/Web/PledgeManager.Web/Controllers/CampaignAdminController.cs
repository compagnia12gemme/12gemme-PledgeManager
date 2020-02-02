using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

    }

}
