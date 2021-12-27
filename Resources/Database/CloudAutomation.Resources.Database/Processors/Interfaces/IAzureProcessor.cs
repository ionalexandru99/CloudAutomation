using System.Threading.Tasks;
using CloudAutomation.Resources.Database.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;

namespace CloudAutomation.Resources.Database.Processors.Interfaces
{
    public interface IAzureProcessor<T> where T : class
    {
        IAzure LoginToAzure();

        bool CreateResource(IAzure azure,
            WorkItem resourceData,
            IResourceGroup resourceGroup,
            out T resource,
            out SqlServerCredentials credentials);
        
        Task<bool> RemoveResource(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup,
            bool generateServer);

        IResourceGroup GetResourceGroup(IAzure azure, WorkItem resourceData);
        
        ISqlServer GetServer(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string serverName);
        
        ISqlServer GetServer(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup);
    }
}