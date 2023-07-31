using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PledgeManager.Web.Controllers {
    
    public class CampaignController : Controller {

        private readonly MongoDatabase _database;
        private readonly MailComposer _composer;
        private readonly ILogger<CampaignController> _logger;

        private const string TempKeyLoginModel = "TempKeyLoginModel";

        public CampaignController(
            MongoDatabase database,
            MailComposer composer,
            ILogger<CampaignController> logger
        ) {
            _database = database;
            _composer = composer;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() {
            return Content("Campaigns root");
        }

        [HttpGet]
        public IActionResult Login(string proceed = null) {
            var vm = this.FromTemp<LoginViewModel>(TempKeyLoginModel) ?? new LoginViewModel();
            if(proceed != null) {
                vm.ProceedUrl = proceed;
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PerformLogin(
            [FromForm] string loginCampaign,
            [FromForm] string loginPassword,
            [FromForm] string proceed
        ) {
            var campaign = await _database.GetCampaign(loginCampaign);
            if(campaign == null) {
                this.AddToTemp(TempKeyLoginModel, new LoginViewModel {
                    LoginFailed = true,
                    ProceedUrl = proceed
                });
                return RedirectToAction(nameof(Index));
            }

            if(!BCrypt.Net.BCrypt.Verify(loginPassword, campaign.PasswordHash)) {
                this.AddToTemp(TempKeyLoginModel, new LoginViewModel {
                    LoginFailed = true,
                    ProceedUrl = proceed
                });
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Administrator for campaign '{0}' logged in", loginCampaign);

            var claims = new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, loginCampaign)
            };

            await HttpContext.SignInAsync(
                Startup.CampaignLoginCookieScheme,
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, Startup.CampaignLoginCookieScheme)
                ),
                new AuthenticationProperties {
                    AllowRefresh = true,
                    IsPersistent = true
                }
            );

            if(proceed != null) {
                return LocalRedirect(proceed);
            }
            else {
                return RedirectToAction("Index", "CampaignAdmin", new {
                    campaignCode = loginCampaign
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(Startup.CampaignLoginCookieScheme);

            return Content("Logout");
        }

    }

}
