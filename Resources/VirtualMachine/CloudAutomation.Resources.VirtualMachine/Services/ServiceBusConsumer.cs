using System;
using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;

namespace CloudAutomation.Resources.VirtualMachine.Services
{
    public abstract class ServiceBusConsumer : BackgroundService
    {
        protected readonly ISubscriptionClient SubscriptionClient;
        protected readonly IResourceProcessor<IVirtualMachine> ResourceProcessor;
        protected readonly ILogger<ServiceBusConsumer> Logger;
        protected readonly IValidationProcessor ValidationProcessor;

        protected const string ResourceType = "Virtual Machine";

        protected ServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<IVirtualMachine> resourceProcessor, 
            ILogger<ServiceBusConsumer> logger,
            IValidationProcessor validationProcessor,
            string topicName)
        {
            SubscriptionClient = ConfigureSubscription(serviceBusConfiguration, topicName);
            ResourceProcessor = resourceProcessor;
            Logger = logger;
            ValidationProcessor = validationProcessor;
        }

        private static ISubscriptionClient ConfigureSubscription(
            IServiceBusConfiguration serviceBusConfiguration,
            string topicName)
        {
            var serviceBusSettings = serviceBusConfiguration.GetServiceBusSettings(topicName);
            
            return new SubscriptionClient(
                serviceBusSettings.ConnectionString,
                serviceBusSettings.Topic,
                serviceBusSettings.Subscription)
            {
                OperationTimeout = new TimeSpan(0,0,30,0)
            };
        }
    }
}