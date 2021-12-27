using System.Threading.Tasks;
using CloudAutomation.Resources.StorageAccount.Clients;
using CloudAutomation.Resources.StorageAccount.Factories;
using CloudAutomation.Resources.StorageAccount.Processors.Interfaces;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resource = CloudAutomation.Models.DevOps.Resource;
using WorkItem = CloudAutomation.Resources.StorageAccount.Models.WorkItem;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.StorageAccount.Processors
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

        public async Task Execute(IStorageAccount azureResource, Resource devOpsResource)
        {
            var resourceData = await _getWorkItem.GetWorkItemByUri(devOpsResource.Url, _devOpsSettings.Pat);
            var emailDetails = _emailFactory.GetEmailDetailsFromResourceData(azureResource, resourceData);

            var result = await _emailClient.SendResourceForConfirmation(emailDetails);

            if (!result)
            {
                _logger.LogError($"An error appeared while trying to send the confirmation email to {emailDetails.Email.Recipient} about the resource with the id {resourceData.Id}");
            }
        }
    }
}