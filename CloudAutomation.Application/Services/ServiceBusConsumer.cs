using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudAutomation.Application.Interfaces;
using CloudAutomation.Models;
using CloudAutomation.Models.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudAutomation.Application.Services
{
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly IWorkItemProcessor _processor;
        private readonly ILogger<ServiceBusConsumer> _logger;
        private readonly ISubscriptionClient _subscriptionClient;

        public ServiceBusConsumer(ISubscriptionClient subscriptionClient, IWorkItemProcessor processor, ILogger<ServiceBusConsumer> logger)
        {
            _subscriptionClient = subscriptionClient;
            _processor = processor;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler(async (message, token) =>
            {
                try
                {
                    var workItem = JsonConvert.DeserializeObject<WorkItem>(Encoding.UTF8.GetString(message.Body));

                    await _processor.Execute(workItem);

                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    _logger.LogInformation($"Completed processing for approval of the work item with id {workItem.Resource.Id}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error appeared while trying to process service bus message");
                    await _subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken);
                }
            }, new MessageHandlerOptions(args => Task.CompletedTask));
            return Task.CompletedTask;
        }
    }
}