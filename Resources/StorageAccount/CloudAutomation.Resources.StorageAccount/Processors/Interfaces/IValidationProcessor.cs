using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.StorageAccount.Processors.Interfaces
{
    public interface IValidationProcessor
    {
        Task Execute(IStorageAccount azureResource, Resource devOpsResource);
    }
}
