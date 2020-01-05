using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public async Task<IActionResult> Index(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token)
        {
            _logger.LogInformation("Loading pledge information for campaign {0} and user {1}", campaign, userId);

            var c = await _database.GetCampaign(campaign);
            if(c == null) {
                _logger.LogInformation("Campaign {0} does not exist", campaign);
                return NotFound();
            }

            var pledge = await _database.GetPledge(c.Id, userId);
            if(pledge == null) {
                _logger.LogInformation("Pledge for user #{0} does not exist", userId);
                return NotFound();
            }
            if(!pledge.Token.Equals(token)) {
                _logger.LogInformation("Token for user #{0} does not match", userId);
                return NotFound();
            }

            return Json(pledge);
        }

    }

}
