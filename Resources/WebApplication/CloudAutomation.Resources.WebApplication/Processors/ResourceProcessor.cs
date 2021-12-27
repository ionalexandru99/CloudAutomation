using System;
using System.Threading.Tasks;
using CloudAutomation.Models.Enums;
using CloudAutomation.Resources.WebApplication.Models;
using CloudAutomation.Resources.WebApplication.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.WebApplication.Processors
{
    public class ResourceProcessor<T> : IResourceProcessor<T> where T : class
    {
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly IAzureProcessor<T> _azureProcessor;
        private readonly DevOpsSettings _devOpsSettings;

        public ResourceProcessor(
            IGetWorkItem<WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IUpdateStateClient updateStateClient,
            IAzureProcessor<T> azureProcessor)
        {
            _getWorkItem = getWorkItem;
            _updateStateClient = updateStateClient;
            _azureProcessor = azureProcessor;
            _devOpsSettings = devOpsSettings.Value;
        }

        public async Task<T> Execute(Resource resource)
        {
            var resourceData = await _getWorkItem.GetWorkItemByUri(resource.Url, _devOpsSettings.Pat);
            await _updateStateClient.Execute(resource.Id.ToString(), State.InProgress);

            var azureLogin = _azureProcessor.LoginToAzure();

            var resourceGroup = _azureProcessor.GetResourceGroup(azureLogin, resourceData);

            var isCreated = _azureProcessor.CreateResource(azureLogin, resourceData, resourceGroup, out var createdResource);

            if (isCreated)
            {
                await _updateStateClient.Execute(resource.Id.ToString(), State.Resolved);
                return createdResource;
            }

            await _updateStateClient.Execute(resource.Id.ToString(), State.Error);
            throw new Exception("An error appeared while trying to  create resource");
        }
    }
}