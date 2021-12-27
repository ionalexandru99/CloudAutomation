using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.VirtualMachine.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(IVirtualMachine azureResource, Resource devOpsResource, string username, string password);
    }
}
