using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudAutomation.Resources.ResourceGroup.Processors;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable once TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.ResourceGroup.Services
{
    public class CreationServiceBusConsumer : ServiceBusConsumer
    {
        private const string TopicName = "resource-creation";
        
        public CreationServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<IResourceGroup> resourceProcessor, 
            ILogger<CreationServiceBusConsumer> logger, 
            IValidationProcessor validationProcessor) 
            : base(serviceBusConfiguration, resourceProcessor, logger, validationProcessor, TopicName) { }
        
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
                        var result = await ResourceProcessor.Execute(workItem);
                        await ValidationProcessor.Execute(result, workItem);
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