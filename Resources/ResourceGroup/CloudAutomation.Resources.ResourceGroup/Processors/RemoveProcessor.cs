using System;
using System.Threading.Tasks;
using CloudAutomation.Models.Enums;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.ResourceGroup.Processors
{
    public class RemoveProcessor : IRemoveProcessor
    {
        private readonly IGetWorkItem<Models.WorkItem> _getWorkItem;
        private readonly DevOpsSettings _devOpsSettings;
        private readonly IAzureProcessor<IResourceGroup> _azureProcessor;

        public RemoveProcessor(
            IGetWorkItem<Models.WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IAzureProcessor<IResourceGroup> azureProcessor)
        {
            _getWorkItem = getWorkItem;
            _devOpsSettings = devOpsSettings.Value;
            _azureProcessor = azureProcessor;
        }
        
        public async Task Execute(Resource resource)
        {
            var resourceData = await _getWorkItem.GetWorkItemByUri(resource.Url, _devOpsSettings.Pat);
            
            var azureLogin = _azureProcessor.LoginToAzure();

            await _azureProcessor.RemoveResource(azureLogin, resourceData);
        }
    }
}