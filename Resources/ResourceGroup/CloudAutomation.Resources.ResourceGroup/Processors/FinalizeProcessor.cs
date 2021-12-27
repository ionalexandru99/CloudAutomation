using System.Threading.Tasks;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.ResourceGroup.Processors
{
    public class FinalizeProcessor : IFinalizeProcessor
    {
        private readonly IGetPickList _getPickList;
        private readonly IUpdatePickList _updatePickList;
        private readonly IGetWorkItem<Models.WorkItem> _getWorkItem;
        private readonly ILogger<FinalizeProcessor> _logger;
        private readonly DevOpsSettings _devOpsSettings;

        private const string PickListId = "f7aa3267-6085-452c-b42e-27faa8213f5d";
        
        public FinalizeProcessor(
            IGetPickList getPickList, 
            IUpdatePickList updatePickList,
            IGetWorkItem<Models.WorkItem> getWorkItem,
            IOptions<DevOpsSettings> devOpsSettings,
            ILogger<FinalizeProcessor> logger)
        {
            _getPickList = getPickList;
            _updatePickList = updatePickList;
            _getWorkItem = getWorkItem;
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
        }
        
        public async Task Execute(Resource resource)
        {
            var workItem = await _getWorkItem.GetWorkItemById(resource.Id.ToString(), _devOpsSettings.Pat);
            var item = GeneratePickListValue(workItem);

            var pickList = await _getPickList.Execute(PickListId);
            pickList.AddItem(item);
            var result = await _updatePickList.Execute(pickList);

            if (!result)
            {
                _logger.LogError("Could not update the Resource Group pick list");
            }
        }

        private static string GeneratePickListValue(Models.WorkItem resource)
        {
            var value = string.Format($"{resource.Id} - {resource.Fields.Name} - {resource.Fields.Manager}");
            return value;
        }
    }
}