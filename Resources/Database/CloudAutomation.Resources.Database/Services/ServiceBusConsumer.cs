﻿using System;
using CloudAutomation.Resources.Database.Processors.Interfaces;
using CloudAutomation.Utils.Configurations;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudAutomation.Resources.Database.Services
{
    public abstract class ServiceBusConsumer : BackgroundService
    {
        protected readonly ISubscriptionClient SubscriptionClient;
        protected readonly IResourceProcessor<ISqlDatabase> ResourceProcessor;
        protected readonly ILogger<ServiceBusConsumer> Logger;
        protected readonly IValidationProcessor ValidationProcessor;

        protected const string ResourceType = "Database";

        protected ServiceBusConsumer(
            IServiceBusConfiguration serviceBusConfiguration, 
            IResourceProcessor<ISqlDatabase> resourceProcessor, 
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
                OperationTimeout = new TimeSpan(0, 0, 30, 0)
            };
        }
    }
}