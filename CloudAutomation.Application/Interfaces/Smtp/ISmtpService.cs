using System.Threading.Tasks;
using CloudAutomation.Models.EmailTemplate;

namespace CloudAutomation.Application.Interfaces.Smtp
{
    public interface ISmtpService
    {
        public Task SendEmail(EmailDetails emailDetails);
    }
}