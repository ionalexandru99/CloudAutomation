using System;
using CloudAutomation.Models.Enums;
using CloudAutomation.Resources.Database.Models;
using CloudAutomation.Resources.Database.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.Database.Processors
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

        public T Execute(Resource resource, out SqlServerCredentials credentials)
        {
            var resourceData = _getWorkItem.GetWorkItemByUri(resource.Url, _devOpsSettings.Pat)
                .GetAwaiter()
                .GetResult();
            _updateStateClient.Execute(resource.Id.ToString(), State.InProgress).GetAwaiter().GetResult();

            var azureLogin = _azureProcessor.LoginToAzure();

            var resourceGroup = _azureProcessor.GetResourceGroup(azureLogin, resourceData);

            var isCreated = _azureProcessor.CreateResource(azureLogin, resourceData, resourceGroup, out var createdResource, out credentials);

            if (isCreated)
            {
                _updateStateClient.Execute(resource.Id.ToString(), State.Resolved).GetAwaiter().GetResult();
                return createdResource;
            }

            _updateStateClient.Execute(resource.Id.ToString(), State.Error).GetAwaiter().GetResult();
            throw new Exception("An error appeared while trying to  create resource");
        }
    }
}