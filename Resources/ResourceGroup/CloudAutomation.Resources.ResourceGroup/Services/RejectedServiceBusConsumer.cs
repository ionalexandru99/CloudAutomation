using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.ResourceGroup.Services
{
    public class RejectedServiceBusConsumer : ServiceBusConsumer
    {
        private readonly IRemoveProcessor _removeProcessor;
        private const string TopicName = "resource-rejected";

        public RejectedServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration,
            IResourceProcessor<IResourceGroup> resourceProcessor,
            ILogger<RejectedServiceBusConsumer> logger,
            IValidationProcessor validationProcessor,
            IRemoveProcessor removeProcessor) :
            base(serviceBusConfiguration, resourceProcessor, logger, validationProcessor, TopicName)
        {
            _removeProcessor = removeProcessor;
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
                        await _removeProcessor.Execute(workItem);
                    }

                    await SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    Logger.LogInformation($"Completed the removal of the resource with id {workItem.Id}");
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