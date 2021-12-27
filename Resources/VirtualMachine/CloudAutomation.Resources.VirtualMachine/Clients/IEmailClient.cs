using System.Threading.Tasks;
using CloudAutomation.Models.EmailTemplate;

namespace CloudAutomation.Resources.VirtualMachine.Clients
{
    public interface IEmailClient
    {
        Task<bool> SendResourceForApproval(Email email);
        Task<bool> SendResourceForConfirmation(CreatedResourceEmail createdResourceEmail);
    }
}
