using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.Database.Models;
using CloudAutomation.Resources.Database.Processors.Interfaces;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.Database.Processors
{
    public class FinalizeProcessor : IFinalizeProcessor
    {
        private readonly IGetPickList _getPickList;
        private readonly IUpdatePickList _updatePickList;
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly ILogger<FinalizeProcessor> _logger;
        private readonly IAzureProcessor<ISqlDatabase> _azureProcessor;
        private readonly DevOpsSettings _devOpsSettings;

        private const string PickListId = "c6cd820a-5186-4062-b7a1-175b23ad7d09";
        public FinalizeProcessor(
            IGetPickList getPickList, 
            IUpdatePickList updatePickList,
            IGetWorkItem<WorkItem> getWorkItem,
            IOptions<DevOpsSettings> devOpsSettings,
            ILogger<FinalizeProcessor> logger,
            IAzureProcessor<ISqlDatabase> azureProcessor)
        {
            _getPickList = getPickList;
            _updatePickList = updatePickList;
            _getWorkItem = getWorkItem;
            _devOpsSettings = devOpsSettings.Value;
            _logger = logger;
            _azureProcessor = azureProcessor;
        }
        
        public async Task Execute(Resource resource)
        {
            var workItem = await _getWorkItem.GetWorkItemById(resource.Id.ToString(), _devOpsSettings.Pat);
            var item = GeneratePickListValue(workItem);

            var pickList = await _getPickList.Execute(PickListId);
            pickList.AddItem(item);
            pickList.Items = pickList.Items.Distinct().ToList();
            var result = await _updatePickList.Execute(pickList);

            if (!result)
            {
                _logger.LogError("Could not update the Resource Group pick list");
            }
        }
        
        private string GeneratePickListValue(WorkItem resource)
        {
            var azure = _azureProcessor.LoginToAzure();
            var resourceGroup = _azureProcessor.GetResourceGroup(azure, resource);
            var server = _azureProcessor.GetServer(azure, resource, resourceGroup);
            
            return server.Name;
        }
    }
}