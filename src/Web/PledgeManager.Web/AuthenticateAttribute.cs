using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace PledgeManager.Web {

    public class AuthenticateAttribute : Attribute, IAsyncActionFilter {

        private readonly string _policy;

        public AuthenticateAttribute(string policy) {
            _policy = policy;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var http = context.HttpContext;
            var policyManager = http.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();
            var policy = await policyManager.GetPolicyAsync(_policy);
            if(policy != null) {
                var policyEvaluator = http.RequestServices.GetRequiredService<IPolicyEvaluator>();
                await policyEvaluator.AuthenticateAsync(policy, http);
            }

            await next();
        }
    }

}
