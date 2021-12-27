using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Resources.Database.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Options;
using WorkItem = CloudAutomation.Resources.Database.Models.WorkItem;

namespace CloudAutomation.Resources.Database.Processors
{
    public class RemoveProcessor : IRemoveProcessor
    {
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly DevOpsSettings _devOpsSettings;
        private readonly IAzureProcessor<ISqlDatabase> _azureProcessor;

        private const string DefaultServerValue = "Select a value";

        public RemoveProcessor(
            IGetWorkItem<WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IAzureProcessor<ISqlDatabase> azureProcessor)
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
            var generateServer = resourceData.Fields.ServerName.Equals(DefaultServerValue);
            
            await _azureProcessor.RemoveResource(azureLogin, resourceData, resourceGroup, generateServer);
        }
    }
}