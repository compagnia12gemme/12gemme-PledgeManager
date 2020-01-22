using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace PledgeManager.Web {

    public class MailerService : BackgroundService {

        private readonly IConfiguration _configuration;
        private readonly IMailerQueue _queue;
        private readonly ILogger<MailerService> _logger;

        public MailerService(
            IConfiguration configuration,
            IMailerQueue queue,
            ILogger<MailerService> logger)
        {
            _configuration = configuration;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            var confSection = _configuration.GetSection("Mail");

            var smtpHost = confSection["Host"];
            var smtpPort = Convert.ToInt32(confSection["Port"]);
            var smtpUser = confSection["Username"];
            var smtpPassword = confSection["Password"];

            _logger.LogDebug("Creating new client for SMTP server {0}:{1} for user {2}", smtpHost, smtpPort, smtpUser);

            using var client = new SmtpClient(smtpHost, smtpPort) {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            _logger.LogInformation("SMTP client ready");

            // Start e-mail processing loop
            while (!stoppingToken.IsCancellationRequested) {
                var message = await _queue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Sending mail to {0} '{1}'", message.To, message.Subject);

                try {
                    await client.SendMailAsync(message);
                    _logger.LogDebug("Mail sent");

                    // Wait 3 seconds to throttle delivery
                    await Task.Delay(3000);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Failed to send email to {0}", message.To);
                }
            }
        }

    }

}
