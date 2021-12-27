using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudAutomation.Resources.StorageAccount.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable once TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.StorageAccount.Services
{
    public class ApprovalServiceBusConsumer: ServiceBusConsumer
    {
        private readonly IFinalizeProcessor _finalizeProcessor;
        private const string TopicName = "resource-approved";
        
        public ApprovalServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<IStorageAccount> resourceProcessor, 
            ILogger<ApprovalServiceBusConsumer> logger, 
            IValidationProcessor validationProcessor,
            IFinalizeProcessor finalizeProcessor) : 
            base(serviceBusConfiguration, resourceProcessor, logger, validationProcessor, TopicName)
        {
            _finalizeProcessor = finalizeProcessor;
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
                        _finalizeProcessor.Execute(workItem);
                    }

                    await SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    Logger.LogInformation($"Completed the finalization of the work item with id {workItem.Id}");
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