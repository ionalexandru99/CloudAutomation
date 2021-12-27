using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using WorkItem = CloudAutomation.Resources.StorageAccount.Models.WorkItem;

namespace CloudAutomation.Resources.StorageAccount.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(IStorageAccount azureResource, WorkItem workItem);
    }
}
