using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using CloudAutomation.Resources.ResourceGroup.Models;

namespace CloudAutomation.Resources.ResourceGroup.Processors.Interfaces
{
    public interface IAzureProcessor<T> where T : class
    {
        IAzure LoginToAzure();
        bool CreateResource(IAzure azure, WorkItem resourceData, out T resource);
        Task<bool> RemoveResource(IAzure azure, WorkItem resourceData);
    }
}