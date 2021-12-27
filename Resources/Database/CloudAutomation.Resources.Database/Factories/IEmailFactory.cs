using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Resources.Database.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using WorkItem = CloudAutomation.Resources.Database.Models.WorkItem;

namespace CloudAutomation.Resources.Database.Factories
{
    public interface IEmailFactory
    {
        Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true);
        CreatedResourceEmail GetEmailDetailsFromResourceData(ISqlDatabase azureResource, WorkItem workItem, SqlServerCredentials credentials);
    }
}
