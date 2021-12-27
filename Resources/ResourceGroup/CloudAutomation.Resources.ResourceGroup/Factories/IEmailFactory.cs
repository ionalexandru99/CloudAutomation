using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using WorkItem = CloudAutomation.Resources.ResourceGroup.Models.WorkItem;

namespace CloudAutomation.Resources.ResourceGroup.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem resourceGroup, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(IResourceGroup azureResource, WorkItem workItem);
    }
}
