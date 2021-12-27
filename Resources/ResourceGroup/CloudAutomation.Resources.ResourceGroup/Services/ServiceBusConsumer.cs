using System;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ISubscriptionClient = Microsoft.Azure.ServiceBus.ISubscriptionClient;
using SubscriptionClient = Microsoft.Azure.ServiceBus.SubscriptionClient;

namespace CloudAutomation.Resources.ResourceGroup.Services
{
    public abstract class ServiceBusConsumer : BackgroundService
    {
        protected readonly ISubscriptionClient SubscriptionClient;
        protected readonly IResourceProcessor<IResourceGroup> ResourceProcessor;
        protected readonly ILogger<ServiceBusConsumer> Logger;
        protected readonly IValidationProcessor ValidationProcessor;

        protected const string ResourceType = "Resource Group";

        protected ServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<IResourceGroup> resourceProcessor, 
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