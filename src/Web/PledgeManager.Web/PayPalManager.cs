using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class PayPalManager {

        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalManager> _logger;

        private readonly object _lockRoot = new object();

        public PayPalManager(
            IConfiguration configuration,
            ILogger<PayPalManager> logger
        ) {
            _configuration = configuration;
            _logger = logger;
        }

        private PayPalEnvironment _environment = null;

        public PayPalEnvironment Environment {
            get {
                if(_environment == null) {
                    lock (_lockRoot) {
                        if (_environment == null) {
                            var section = _configuration.GetSection("PayPal");
                            var clientId = section["ClientID"];
                            var clientSecret = section["ClientSecret"];
                            if (Convert.ToBoolean(section["Sandbox"]) == true) {
                                _logger.LogInformation("Creating sandbox PayPal environment for client ID {0}", clientId);

                                _environment = new SandboxEnvironment(clientId, clientSecret);
                            }
                            else {
                                _logger.LogInformation("Creating live PayPal environment for client ID {0}", clientId);

                                _environment = new LiveEnvironment(clientId, clientSecret);
                            }
                        }
                    }
                }
                return _environment;
            }
        }

        private PayPalHttpClient _client = null;

        public PayPalHttpClient Client {
            get {
                if(_client == null) {
                    lock (_lockRoot) {
                        if (_client == null) {
                            _client = new PayPalHttpClient(Environment);
                        }
                    }
                }
                return _client;
            }
        }

    }

}
