using System.Threading.Tasks;
using CloudAutomation.Resources.VirtualMachine.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace CloudAutomation.Resources.VirtualMachine.Processors.Interfaces
{
    public interface IAzureProcessor<T> where T : class
    {
        IAzure LoginToAzure();

        bool CreateResource(
            IAzure azure, 
            WorkItem resourceData, 
            IResourceGroup resourceGroup, 
            string username,
            string password, 
            out T resource);
        
        Task<bool> RemoveResource(IAzure azure, IResourceGroup resourceGroup, WorkItem resourceData);

        IResourceGroup GetResourceGroup(IAzure azure, WorkItem resourceData);
    }
}