using System.Threading.Tasks;
using CloudAutomation.Resources.WebApplication.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace CloudAutomation.Resources.WebApplication.Processors.Interfaces
{
    public interface IAzureProcessor<T> where T : class
    {
        IAzure LoginToAzure();

        bool CreateResource(IAzure azure,
            WorkItem resourceData,
            IResourceGroup resourceGroup,
            out T resource);
        
        Task<bool> RemoveResource(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup);

        IResourceGroup GetResourceGroup(IAzure azure, WorkItem resourceData);
    }
}