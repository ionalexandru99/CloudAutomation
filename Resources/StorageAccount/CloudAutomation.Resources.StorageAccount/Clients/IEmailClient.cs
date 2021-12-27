using System.Threading.Tasks;
using CloudAutomation.Models.EmailTemplate;

namespace CloudAutomation.Resources.StorageAccount.Clients
{
    public interface IEmailClient
    {
        Task<bool> SendResourceForApproval(Email email);
        Task<bool> SendResourceForConfirmation(CreatedResourceEmail createdResourceEmail);
    }
}
