using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class MailComposer {

        private readonly IMailerQueue _queue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailComposer> _logger;

        private readonly MailAddress _mailFrom;
        private readonly MailAddress _mailBcc = null;

        public MailComposer(
            IMailerQueue queue,
            IConfiguration configuration,
            ILogger<MailComposer> logger
        ) {
            _queue = queue;
            _configuration = configuration;
            _logger = logger;

            var confSection = _configuration.GetSection("Mail");

            var mailFromAddress = confSection["FromMail"];
            var mailFromName = confSection["FromName"];
            _mailFrom = new MailAddress(mailFromAddress, mailFromName);
            _logger.LogDebug("Mails will be sent from {0}", _mailFrom);
            
            var mailShadowBccAddress = confSection["ShadowBccMail"];
            if (!string.IsNullOrEmpty(mailShadowBccAddress)) {
                _mailBcc = new MailAddress(mailShadowBccAddress);
                _logger.LogDebug("Sending shadow copy to {0}", mailShadowBccAddress);
            }
        }

        public void SendInvitation(Campaign campaign, Pledge pledge) {
            if(pledge.Email == null) {
                _logger.LogError("Cannot send email to pledge #{0}, no email given", pledge.UserId);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("Ciao{0}!\n",
                string.IsNullOrWhiteSpace(pledge?.Shipping?.Name) ? string.Empty : (", " + pledge.Shipping.Name));
            sb.Append("È finalmente arrivato il momento di definire in maniera esatta la tua ricompensa per aver partecipato alla nostra campagna di crowdfunding.\n\n");
            sb.Append("Ti preghiamo di cliccare sul collegamento qui sotto per accedere al pannello di gestione:\n");
            sb.AppendFormat("{0}/campaign/{1}/pledge/{2}/{3}\n\n",
                Environment.GetEnvironmentVariable("LINK_BASE"),
                campaign.Code, pledge.UserId, pledge.UserToken);
            sb.Append("Il pannello di gestione della tua offerta ti permetterà di determinare il livello finale della tua ricompensa ed aggiungere gli articoli aggiuntivi che desideri.Puoi eventualmente anche decidere di aumentare la tua offerta iniziale, in modo da aggiungere più articoli.\n\n");
            sb.Append("Grazie ancora per il tuo contributo!\n\n");
            sb.Append(campaign.MailSignature);

            SendMessage(pledge.Email,
                $"🗳 Accesso al pledge manager di {campaign.Title}",
                sb.ToString()
            );
        }

        private void SendMessage(string recipientAddress, string subject, string contents) {
            var msg = new MailMessage {
                From = _mailFrom,
                IsBodyHtml = false,
                Subject = subject,
                Body = contents
            };
            msg.To.Add(recipientAddress);
            
            if(_mailBcc != null) {
                msg.Bcc.Add(_mailBcc);
            }

            _queue.Enqueue(msg);
        }

    }

    public static class MailComposerExtensions {

        public static IServiceCollection AddMailComposer(this IServiceCollection services) {
            services.AddSingleton<IMailerQueue, MailerQueue>();
            services.AddHostedService<MailerService>();
            services.AddSingleton<MailComposer>();
            return services;
        }

    }

}
