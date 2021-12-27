using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using WorkItem = CloudAutomation.Resources.VirtualMachine.Models.WorkItem;

namespace CloudAutomation.Resources.VirtualMachine.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(IVirtualMachine azureResource, WorkItem workItem,
            string username, string password);
    }
}
