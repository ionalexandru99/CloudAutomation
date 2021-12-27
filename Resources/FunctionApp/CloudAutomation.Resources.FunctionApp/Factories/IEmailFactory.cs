using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using WorkItem = CloudAutomation.Resources.FunctionApp.Models.WorkItem;

namespace CloudAutomation.Resources.FunctionApp.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(IFunctionApp azureResource, WorkItem workItem);
    }
}
