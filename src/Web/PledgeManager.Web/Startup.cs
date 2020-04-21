using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace PledgeManager.Web {

    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public const string CampaignLoginCookieScheme = "CampaignLoginCookieScheme";
        public const string CampaignLoginPolicy = "CampaignLoginPolicy";

        public void ConfigureServices(IServiceCollection services) {
            services.AddRouting(options => {
                options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
            });
            services.AddControllersWithViews();

            services.AddSingleton(typeof(MongoDatabase));
            services.AddSingleton(typeof(PayPalManager));

            services.AddMailComposer();

            services.AddAuthentication()
                .AddCookie(CampaignLoginCookieScheme, options => {
                    options.LoginPath = "/campaign/login";
                    options.LogoutPath = "/campaign/logout";
                    options.ReturnUrlParameter = "proceed";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.Cookie = new CookieBuilder {
                        Domain = "12gem.me",
                        IsEssential = true,
                        Name = "PledgeManagerCampaignLogin",
                        SecurePolicy = CookieSecurePolicy.None,
                        SameSite = SameSiteMode.Strict,
                        HttpOnly = true
                    };
                });
            services.AddAuthorization(options => {
                options.AddPolicy(
                    CampaignLoginPolicy,
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(CampaignLoginCookieScheme)
                        .Build()
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRequestLocalization(o => {
                o.AddSupportedCultures("it");
                o.AddSupportedUICultures("it");
                o.DefaultRequestCulture = new RequestCulture("it");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute("campaignRoot",
                    "campaign/{action:slugify}",
                    defaults: new {
                        controller = "Campaign",
                        action = "Index"
                    });

                endpoints.MapControllerRoute("campaignAdmin",
                    "campaign/{campaignCode}/admin/{action:slugify}",
                    defaults: new {
                        controller = "CampaignAdmin",
                        action = "Index"
                    });

                endpoints.MapControllerRoute("pledge",
                    "campaign/{campaignCode}/pledge/{userId:int:min(1)}/{token}/{action:slugify}",
                    defaults: new {
                        controller = "Pledge",
                        action = "Index"
                    });

                endpoints.MapControllerRoute("root",
                    "{action:slugify}",
                    defaults: new {
                        controller = "Home"
                    });
            });
        }
    }
}
