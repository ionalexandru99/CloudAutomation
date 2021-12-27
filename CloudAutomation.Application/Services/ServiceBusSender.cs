using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using CloudAutomation.Models.Enums;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Utils.Configurations;
using CloudAutomation.Utils.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudAutomation.Application.Services
{
    public class ServiceBusSender : IServiceBusSender
    {
        private readonly ILogger<ServiceBusSender> _logger;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly IServiceBusConfiguration _serviceBusConfiguration;

        public ServiceBusSender(
            IServiceBusConfiguration serviceBusConfiguration, 
            ILogger<ServiceBusSender> logger, 
            IUpdateStateClient updateStateClient)
        {
            _logger = logger;
            _updateStateClient = updateStateClient;
            _serviceBusConfiguration = serviceBusConfiguration;
        }

        public async Task SendMessage(Resource workItem, string topicName)
        {
            var serviceBusSettings = _serviceBusConfiguration.GetServiceBusSettings(topicName);

            try
            {
                await using (var client = new ServiceBusClient(serviceBusSettings.ConnectionString))
                {
                    var sender = client.CreateSender(serviceBusSettings.Topic);
                    var resourceData = JsonConvert.SerializeObject(workItem);
                    await sender.SendMessageAsync(new ServiceBusMessage(resourceData));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to send message");
                await _updateStateClient.Execute(workItem.Id.ToString(), State.Error);
            }
        }
    }
}