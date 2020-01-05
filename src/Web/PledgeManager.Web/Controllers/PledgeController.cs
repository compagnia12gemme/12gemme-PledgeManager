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

        public async Task<IActionResult> Index(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token)
        {
            _logger.LogInformation("Loading pledge information for campaign {0} and user {1}", campaign, userId);

            _logger.LogInformation("Campaign is mapped {0}",
                MongoDB.Bson.Serialization.BsonClassMap.IsClassMapRegistered(typeof(Campaign))
            );

            var map = MongoDB.Bson.Serialization.BsonClassMap.LookupClassMap(typeof(Campaign));
            _logger.LogInformation("Campaign member map: {0}",
                string.Join(", ", from m in map.AllMemberMaps select (m.MemberName + "=>" + m.ElementName))
            );

            (var c, var pledge) = await GetPledge(campaign, userId);
            if(c == null || pledge == null) {
                return NotFound();
            }
            if(!pledge.UserToken.Equals(token)) {
                _logger.LogInformation("Token for user #{0} does not match", userId);
                return Unauthorized();
            }

            return View("Show", new PledgeShowViewModel {
                Campaign = c,
                Pledge = pledge
            });
        }

    }

}
