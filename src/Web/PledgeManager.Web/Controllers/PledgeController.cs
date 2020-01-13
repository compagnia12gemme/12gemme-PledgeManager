using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.Models;
using PledgeManager.Web.RequestModels;
using PledgeManager.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PledgeManager.Web.Controllers {

    public class PledgeController : Controller {

        private readonly MongoDatabase _database;
        private readonly PayPalManager _paypal;
        private readonly ILogger<PledgeController> _logger;

        public PledgeController(
            MongoDatabase database,
            PayPalManager paypal,
            ILogger<PledgeController> logger
        ) {
            _database = database;
            _paypal = paypal;
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

            if(pledge.IsClosed) {
                return View("ShowClosed");
            }

            // Rapid access reward and add-on maps
            var rewardMap = c.Rewards.ToDictionary(reward => reward.Code);
            var addonMap = c.AddOns.ToDictionary(addon => addon.Code);

            // Pick rewards
            var originalReward = rewardMap[pledge.OriginalRewardLevel];
            var currentReward = rewardMap[pledge.CurrentRewardLevel];

            // Compute final total cost of pledge
            var finalCost = currentReward.PledgeBase + (from addon in pledge.AddOns
                                                        let campaignAddon = addonMap[addon.Code]
                                                        select campaignAddon.Cost).Sum();

            // Compute rapid access hashmap of excluded add-ons that cannot be added
            var addedAddOns = from addon in pledge.AddOns
                              let campaignAddon = addonMap[addon.Code]
                              select (campaignAddon, addon.Variant);
            var excludedAddOnCodes = new HashSet<string>();
            foreach (var (addon, _) in addedAddOns) {
                if (!addon.MultipleEnabled) {
                    excludedAddOnCodes.Add(addon.Code);
                }
                if (addon.Excludes != null) {
                    excludedAddOnCodes.UnionWith(addon.Excludes);
                }
            }

            var vm = new PledgeShowViewModel {
                Campaign = c,
                Pledge = pledge,
                CurrentReward = currentReward,
                AddedAddOns = addedAddOns,
                AvailableAddOns = from addon in c.AddOns
                                  where !excludedAddOnCodes.Contains(addon.Code)
                                  select addon,
                UpgradePaths = from upgrade in originalReward.FullUpgradePaths
                               let campaignUpgrade = rewardMap[upgrade]
                               let upgradeDifference = campaignUpgrade.PledgeBase - currentReward.PledgeBase
                               select (campaignUpgrade, upgradeDifference),
                FinalCost = finalCost
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
                campaign,
                userId,
                token
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

            if (!c.Rewards.Any(r => r.Code == rewardCode)) {
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
            if (addon == null) {
                return Content("AddOn code does not exist");
            }
            if (addon.Variants != null) {
                if (!addon.Variants.Any(v => v == variant)) {
                    return Content("AddOn requires variant but no valid variant code supplied");
                }
            }
            else {
                // Fix variant to null in case add-on has no variants
                variant = null;
            }

            var existingCount = pledge.AddOns.Where(a => a.Code == addonCode).Count();
            if (existingCount > 0 && !addon.MultipleEnabled) {
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
            if (addon == null) {
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

        [HttpPost]
        public async Task<IActionResult> ClosePledge(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromForm] string pledgeNotes
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            pledge.Note = pledgeNotes;
            pledge.IsClosed = true;
            pledge.LastUpdate = DateTime.UtcNow;
            await _database.UpdatePledge(pledge);

            return RedirectToAction(nameof(Index), new {
                campaign,
                userId,
                token
            });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token,
            [FromBody] ProcessPaymentRequest paymentRequest
        ) {
            (var c, var pledge, var ret) = await GetPledgeAndVerify(campaign, userId, token);
            if (ret != null) {
                return ret;
            }

            _logger.LogInformation("Processing payment order ID {0} for campaign {1} user ID {2}",
                paymentRequest.OrderId, campaign, userId);

            var reqOrder = new PayPalCheckoutSdk.Orders.OrdersGetRequest(paymentRequest.OrderId);
            var response = await _paypal.Client.Execute(reqOrder);
            if(response.StatusCode != HttpStatusCode.OK) {
                _logger.LogError("Failed to fetch order ID {0}, request status {1}", paymentRequest.OrderId, response.StatusCode);
                return Content("Failed to fetch order from PayPal");
            }
            var order = response.Result<PayPalCheckoutSdk.Orders.Order>();
            _logger.LogInformation("Order ID {0} retrieved with status {1}", paymentRequest.OrderId, order.Status);

            if(!"COMPLETED".Equals(order.Status, StringComparison.InvariantCultureIgnoreCase)) {
                _logger.LogWarning("Order ID {0} not marked as completed", paymentRequest.OrderId);
                return Content("Order not completed");
            }

            decimal total = 0M;
            foreach(var pu in order.PurchaseUnits) {
                _logger.LogDebug("Purchase unit {0}: {1} {2}",
                    pu.Id, pu.AmountWithBreakdown.Value, pu.AmountWithBreakdown.CurrencyCode);
                if(pu.AmountWithBreakdown.CurrencyCode != "EUR") {
                    _logger.LogWarning("Currency {0} not handled", pu.AmountWithBreakdown.CurrencyCode);
                    continue;
                }
                if(!decimal.TryParse(pu.AmountWithBreakdown.Value, out decimal purchaseValue)) {
                    _logger.LogError("Cannot parse purchase unit value {0}", pu.AmountWithBreakdown.Value);
                    continue;
                }
                total += purchaseValue;
            }

            pledge.CurrentPledge += total;
            pledge.Payments.Add(new PledgePayment {
                OrderId = order.Id,
                PayerId = order.Payer.PayerId,
                PayerEmail = order.Payer.Email,
                Value = total,
                Timestamp = DateTime.UtcNow
            });
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
