using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CloudAutomation.Application.Interfaces.Smtp;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Application.Implementations.Smtp
{
    public class SmtpService : ISmtpService
    {
        private readonly EmailAccount _emailAccount;

        public SmtpService(IOptions<EmailAccount> emailAccount)
        {
            _emailAccount = emailAccount.Value;
        }

        public async Task SendEmail(EmailDetails emailDetails)
        {
            var message = GenerateMailMessage(emailDetails);
            var smtp = GenerateSmtpClient();

            await smtp.SendMailAsync(message);
        }

        private MailMessage GenerateMailMessage(EmailDetails emailDetails)
        {
            return new MailMessage
            {
                From = new MailAddress(_emailAccount.Email),
                To = {new MailAddress(emailDetails.Recipient)},
                Subject = emailDetails.Subject,
                IsBodyHtml = SmtpSettings.IsBodyHtml,
                Body = emailDetails.Body
            };
        }

        private SmtpClient GenerateSmtpClient()
        {
            return new SmtpClient
            {
                Port = SmtpSettings.Port,
                Host = SmtpSettings.Host,
                EnableSsl = SmtpSettings.EnableSsl,
                UseDefaultCredentials = SmtpSettings.UseDefaultCredentials,
                Credentials = new NetworkCredential(_emailAccount.Email, _emailAccount.Password),
                DeliveryMethod = SmtpSettings.DeliveryMethod
            };
        }
    }
}