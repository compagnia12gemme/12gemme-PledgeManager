using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        private readonly LinkGenerator _linkGenerator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailComposer> _logger;

        private readonly MailAddress _mailFrom;
        private readonly MailAddress _mailBcc = null;

        public MailComposer(
            IMailerQueue queue,
            LinkGenerator linkGenerator,
            IConfiguration configuration,
            ILogger<MailComposer> logger
        ) {
            _queue = queue;
            _linkGenerator = linkGenerator;
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
            sb.Append(GetGreeting("Ciao", pledge?.Shipping));
            sb.Append("\n");
            sb.Append("È finalmente arrivato il momento di definire in maniera esatta la tua ricompensa per aver partecipato alla nostra campagna di crowdfunding.\n\n");
            sb.Append("Clicca sul collegamento qui sotto per accedere al tuo pannello di gestione:\n");
            sb.Append(GetPledgeManagerLink(campaign, pledge));
            sb.Append("\n\n");
            sb.Append("Il pannello mostrerà la ricompensa che hai originariamente selezionato durante la campagna di crowdfunding e tiene conto dell’eventuale contributo in eccesso che hai versato. Potrai definire gli articoli aggiuntivi che vuoi ricevere oppure sfruttare questa ultima occasione per passare ad un livello di ricompensa superiore.\n\n");
            sb.Append("Grazie ancora per il tuo contributo!\n\n");
            sb.Append(campaign.MailSignature);

            SendMessage(pledge.Email,
                $"📣 Accesso al pledge manager di {campaign.Title}",
                sb.ToString()
            );
        }

        public void SendReminder(Campaign campaign, Pledge pledge) {
            if (pledge.Email == null) {
                _logger.LogError("Cannot send email to pledge #{0}, no email given", pledge.UserId);
                return;
            }

            var sb = new StringBuilder();
            sb.Append(GetGreeting("Ciao", pledge?.Shipping));
            sb.Append("\n");
            sb.Append("La tua ricompensa per la campagna di crowdfunding non è ancora stata finalizzata: questo significa che le informazioni sulla ricompensa e gli articoli aggiuntivi che desideri ottenere ed il tuo indirizzo di spedizione non sono ancora stati registrati.\n\n");
            sb.Append("Clicca sul collegamento qui sotto per accedere al tuo pannello di gestione:\n");
            sb.Append(GetPledgeManagerLink(campaign, pledge));
            sb.Append("\n\n");
            sb.Append("Il pannello mostrerà la ricompensa che hai originariamente selezionato durante la campagna di crowdfunding e tiene conto dell’eventuale contributo in eccesso che hai versato. Potrai definire gli articoli aggiuntivi che vuoi ricevere oppure sfruttare questa ultima occasione per passare ad un livello di ricompensa superiore.\n\n");
            sb.Append("Grazie ancora per il tuo contributo!\n\n");
            sb.Append(campaign.MailSignature);

            SendMessage(pledge.Email,
                $"⏰ Finalizza la tua ricompensa per {campaign.Title}",
                sb.ToString()
            );
        }

        public void SendClosingConfirmation(Campaign campaign, Pledge pledge) {
            if (pledge.Email == null) {
                _logger.LogError("Cannot send email to pledge #{0}, no email given", pledge.UserId);
                return;
            }

            var sb = new StringBuilder();
            sb.Append(GetGreeting("Grazie", pledge?.Shipping));
            sb.Append("\n");
            sb.Append("La tua ricompensa è stata registrata in maniera definitiva.\n\n");
            sb.Append("Puoi accedere in qualsiasi momento al riassunto della tua ricompensa seguendo questo collegamento:\n");
            sb.Append(GetPledgeManagerLink(campaign, pledge));
            sb.Append("\n\n");
            sb.Append("Grazie nuovamente per il tuo contributo!\n\n");
            sb.Append(campaign.MailSignature);

            SendMessage(pledge.Email,
                $"✔ Pledge per {campaign.Title} finalizzato",
                sb.ToString()
            );
        }

        private string GetGreeting(string greeting, ShippingInfo shippingInfo) {
            if(shippingInfo == null ||
               string.IsNullOrWhiteSpace(shippingInfo.GivenName)) {
                return string.Format("{0}!", greeting);
            }
            else {
                return string.Format("{0}, {1}!", greeting, shippingInfo.GivenName);
            }
        }

        private string GetPledgeManagerLink(Campaign campaign, Pledge pledge) {
            return _linkGenerator.GetUriByAction(
                nameof(Controllers.PledgeController.Index),
                "Pledge",
                new {
                    campaignCode = campaign.Code,
                    userId = pledge.UserId,
                    token = pledge.UserToken
                },
                "https",
                new HostString(Environment.GetEnvironmentVariable("SELF_HOST"))
            );
        }

        private void SendMessage(string recipientAddress, string subject, string contents) {
            var msg = new MailMessage {
                From = _mailFrom,
                Sender = _mailFrom,
                IsBodyHtml = false,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = contents,
                BodyEncoding = Encoding.UTF8
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
