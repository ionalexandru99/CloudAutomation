using System.Threading.Tasks;
using CloudAutomation.Resources.Database.Models;
using Microsoft.Azure.Management.Sql.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.Database.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(ISqlDatabase azureResource, Resource devOpsResource, SqlServerCredentials credentials);
    }
}
