using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.FunctionApp.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(IFunctionApp azureResource, Resource devOpsResource);
    }
}
