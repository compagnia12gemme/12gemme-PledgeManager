using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web.Controllers {

    public class PledgeController : Controller {

        private readonly ILogger<PledgeController> _logger;

        public PledgeController(ILogger<PledgeController> logger) {
            _logger = logger;
        }

        public IActionResult Index(
            [FromRoute] string campaign,
            [FromRoute] int userId,
            [FromRoute] string token)
        {
            _logger.LogInformation("Loading pledge information for campaign {0} and user {1}", campaign, userId);

            return Content($"Pledge/Index campaign {campaign} user {userId} token {token}");
        }

    }

}
