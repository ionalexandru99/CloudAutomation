using System.Threading.Tasks;
using CloudAutomation.Resources.VirtualMachine.Clients;
using CloudAutomation.Resources.VirtualMachine.Factories;
using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;
using WorkItem = CloudAutomation.Resources.VirtualMachine.Models.WorkItem;

namespace CloudAutomation.Resources.VirtualMachine.Processors
{
    public class ValidationProcessor : IValidationProcessor
    {
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly IEmailFactory _emailFactory;
        private readonly IEmailClient _emailClient;
        private readonly ILogger<ValidationProcessor> _logger;
        private readonly DevOpsSettings _devOpsSettings;

        public ValidationProcessor(
            IGetWorkItem<WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IEmailFactory emailFactory,
            IEmailClient emailClient,
            ILogger<ValidationProcessor> logger)
        {
            _getWorkItem = getWorkItem;
            _emailFactory = emailFactory;
            _emailClient = emailClient;
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
        }

        public async Task Execute(IVirtualMachine azureResource, Resource devOpsResource, string username, string password)
        {
            var resourceData = await _getWorkItem.GetWorkItemByUri(devOpsResource.Url, _devOpsSettings.Pat);
            var emailDetails = _emailFactory.GetEmailDetailsFromResourceData(azureResource, resourceData, username, password);

            var result = await _emailClient.SendResourceForConfirmation(emailDetails);

            if (!result)
            {
                _logger.LogError($"An error appeared while trying to send the confirmation email to {emailDetails.Email.Recipient} about the resource with the id {resourceData.Id}");
            }
        }
    }
}