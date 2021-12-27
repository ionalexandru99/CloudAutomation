using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using WorkItem = CloudAutomation.Resources.WebApplication.Models.WorkItem;

namespace CloudAutomation.Resources.WebApplication.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(IWebApp azureResource, WorkItem workItem);
    }
}
