using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Resources.FunctionApp.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Extensions.Options;
using WorkItem = CloudAutomation.Resources.FunctionApp.Models.WorkItem;

namespace CloudAutomation.Resources.FunctionApp.Processors
{
    public class RemoveProcessor : IRemoveProcessor
    {
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly DevOpsSettings _devOpsSettings;
        private readonly IAzureProcessor<IFunctionApp> _azureProcessor;

        public RemoveProcessor(
            IGetWorkItem<WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IAzureProcessor<IFunctionApp> azureProcessor)
        {
            _getWorkItem = getWorkItem;
            _devOpsSettings = devOpsSettings.Value;
            _azureProcessor = azureProcessor;
        }

        public async Task Execute(Resource resource)
        {
            var resourceData = await _getWorkItem.GetWorkItemByUri(resource.Url, _devOpsSettings.Pat);
            
            var azureLogin = _azureProcessor.LoginToAzure();
            
            var resourceGroup = _azureProcessor.GetResourceGroup(azureLogin, resourceData);
            
            await _azureProcessor.RemoveResource(azureLogin, resourceData, resourceGroup);
        }
    }
}