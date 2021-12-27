using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudAutomation.Resources.VirtualMachine.Models;
using CloudAutomation.Resources.VirtualMachine.Processors;
using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable once TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.VirtualMachine.Services
{
    public class CreationServiceBusConsumer : ServiceBusConsumer
    {
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly IPasswordProcessor _passwordProcessor;
        private readonly DevOpsSettings _devOpsSettings;
        
        private const string TopicName = "resource-creation";
        
        public CreationServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<IVirtualMachine> resourceProcessor, 
            ILogger<CreationServiceBusConsumer> logger, 
            IValidationProcessor validationProcessor,
            IGetWorkItem<WorkItem> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IPasswordProcessor passwordProcessor) 
            : base(serviceBusConfiguration, resourceProcessor, logger, validationProcessor, TopicName)
        {
            _getWorkItem = getWorkItem;
            _passwordProcessor = passwordProcessor;
            _devOpsSettings = devOpsSettings.Value;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SubscriptionClient.RegisterMessageHandler(async (message, token) =>
            {
                try
                {
                    var workItem = JsonConvert
                        .DeserializeObject<CloudAutomation.Models.DevOps.Resource>
                            (Encoding.UTF8.GetString(message.Body));

                    if (workItem.Fields.WorkItemType == ResourceType)
                    {
                        var resourceData = await _getWorkItem.GetWorkItemByUri(workItem.Url, _devOpsSettings.Pat);
                        var username = SdkContext.RandomResourceName(resourceData.Fields.Name[..5], 10);
                        var password = _passwordProcessor
                            .GeneratePassword(true, true, true, false, 15);
                        var result = await ResourceProcessor.Execute(workItem, username, password);
                        
                        await ValidationProcessor.Execute(result, workItem, username, password);
                    }

                    await SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    Logger.LogInformation($"Completed creation of the work item with id {workItem.Id}");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "An error appeared while trying to process service bus message");
                    await SubscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken);
                }
            }, new MessageHandlerOptions(args => Task.CompletedTask));
            return Task.CompletedTask;
        }
    }
}