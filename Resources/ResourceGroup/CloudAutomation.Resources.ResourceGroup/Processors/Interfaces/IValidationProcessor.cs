using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.ResourceGroup.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(IResourceGroup azureResource, Resource devOpsResource);
    }
}
