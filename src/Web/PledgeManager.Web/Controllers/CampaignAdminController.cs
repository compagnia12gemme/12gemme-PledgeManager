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

        public CampaignAdminController(
            MongoDatabase database,
            MailComposer composer,
            ILogger<CampaignAdminController> logger
        ) {
            _database = database;
            _composer = composer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromRoute] string campaignCode
        ) {
            var campaign = await _database.GetCampaign(campaignCode);

            (var pledges, var closedCount) = await _database.GetPledges(campaign.Id);

            var vm = new CampaignDashboardViewModel {
                Campaign = campaign,
                Pledges = pledges,
                PledgeCount = pledges.Count,
                ClosedPledgeCount = (int)closedCount
            };

            return View("Home", vm);
        }

    }

}
