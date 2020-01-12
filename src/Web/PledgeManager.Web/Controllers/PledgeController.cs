using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.Models;
using PledgeManager.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Controllers {

    public class PledgeController : Controller {

        private readonly MongoDatabase _database;
        private readonly ILogger<PledgeController> _logger;

        public PledgeController(MongoDatabase database, ILogger<PledgeController> logger) {
            _database = database;
            _logger = logger;
        }

        private async Task<(Campaign, Pledge)> GetPledge(string campaign, int userId) {
            var c = await _database.GetCampaign(campaign);
            if (c == null) {
                _logger.LogInformation("Campaign {0} does not exist", campaign);
                return (null, null);
            }

            var pledge = await _database.GetPledge(c.Id, userId);
            if (pledge == null) {
                _logger.LogInformation("Pledge for user #{0} does not exist", userId);
                return (null, null);
            }

            return (c, pledge);
        }

        private async Task<(Campaign, Pledge, IActionResult)> GetPledgeAndVerify(
            string campaign, int userId, string token) {

            _logger.LogInformation("Loading pledge information for campaign {0} and user {1}", campaign, userId);

            (var c, var pledge) = await GetPledge(campaign, userId);
            if (c == null || pledge == null) {
                return (null, null, NotFound());
            }
            if (!pledge.UserToken.Equals(token)) {
                _logger.LogInformation("Token for user #{0} does not match", userId);
                return (null, null, Unauthorized());
            }

            return (c, pledge, null);
        }

        public async Task<IActionResult> Index(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            var rewardMap = c.Rewards.ToDictionary(reward => reward.Code);
            var addonMap = c.AddOns.ToDictionary(addon => addon.Code);
            var originalReward = rewardMap[pledge.OriginalRewardLevel];
            var currentReward = rewardMap[pledge.CurrentRewardLevel];

            var finalCost = currentReward.PledgeBase + (from addon in pledge.AddOns
                                                        let campaignAddon = addonMap[addon.Code]
                                                        select campaignAddon.Cost).Sum();

            var vm = new PledgeShowViewModel {
                Campaign = c,
                Pledge = pledge,
                CurrentReward = currentReward,
                AddOns = from addon in pledge.AddOns
                         let campaignAddon = addonMap[addon.Code]
                         select (campaignAddon, addon.Variant),
                UpgradePaths = from upgrade in originalReward.FullUpgradePaths
                               let campaignUpgrade = rewardMap[upgrade]
                               let upgradeDifference = campaignUpgrade.PledgeBase - currentReward.PledgeBase
                               select (campaignUpgrade, upgradeDifference),
                FinalCost = finalCost,
                CanBeClosed = pledge.CurrentPledge >= finalCost
            };

            return View("Show", vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShipping(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromForm] string inputShippingAddress,
            [FromForm] string inputShippingZip,
            [FromForm] string inputShippingCity,
            [FromForm] string inputShippingProvince,
            [FromForm] string inputShippingCountry
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            pledge.Shipping = new ShippingInfo {
                Address = inputShippingAddress,
                ZipCode = inputShippingZip,
                City = inputShippingCity,
                Province = inputShippingProvince,
                Country = inputShippingCountry
            };
            pledge.LastUpdate = DateTime.UtcNow;
            await _database.UpdatePledge(pledge);

            return RedirectToAction(nameof(Index), new {
                campaign, userId, token
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReward(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromForm] string rewardCode
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            if(!c.Rewards.Any(r => r.Code == rewardCode)) {
                return Content("Reward code does not exist");
            }

            pledge.CurrentRewardLevel = rewardCode;
            pledge.LastUpdate = DateTime.UtcNow;
            await _database.UpdatePledge(pledge);

            return RedirectToAction(nameof(Index), new {
                campaign,
                userId,
                token
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddAddOn(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromForm] string addonCode,
            [FromForm] string variant
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            var addon = c.AddOns.Where(a => a.Code == addonCode).SingleOrDefault();
            if(addon == null) {
                return Content("AddOn code does not exist");
            }
            if(addon.Variants != null) {
                if(!addon.Variants.Any(v => v == variant)) {
                    return Content("AddOn requires variant but no valid variant code supplied");
                }
            }
            else {
                // Fix variant to null in case add-on has no variants
                variant = null;
            }

            var existingCount = pledge.AddOns.Where(a => a.Code == addonCode).Count();
            if(existingCount > 0 && !addon.MultipleEnabled) {
                return Content("Cannot add multiple addons with code {0}", addonCode);
            }

            pledge.AddOns.Add(new PledgeAddOn {
                Code = addonCode,
                Variant = variant
            });
            pledge.LastUpdate = DateTime.UtcNow;
            await _database.UpdatePledge(pledge);

            return RedirectToAction(nameof(Index), new {
                campaign,
                userId,
                token
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAddOn(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromForm] string addonCode,
            [FromForm] string variant
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            var addon = (from a in pledge.AddOns
                         where a.Code == addonCode && (variant == null || variant == a.Variant)
                         select a).FirstOrDefault();
            if(addon == null) {
                return Content("Addon is not in pledge and cannot be removed");
            }
            
            pledge.AddOns.Remove(addon);
            pledge.LastUpdate = DateTime.UtcNow;
            await _database.UpdatePledge(pledge);

            return RedirectToAction(nameof(Index), new {
                campaign,
                userId,
                token
            });
        }

    }

}
