using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.WebApplication.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(IWebApp azureResource, Resource devOpsResource);
    }
}
