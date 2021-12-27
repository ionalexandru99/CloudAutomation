using System.Linq;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Utils.Configurations
{
    public class ServiceBusConfiguration : IServiceBusConfiguration
    {
        private readonly ServiceBusAccounts _serviceBusAccounts;

        public ServiceBusConfiguration(IOptions<ServiceBusAccounts> serviceBusAccounts)
        {
            _serviceBusAccounts = serviceBusAccounts.Value;
        }
        
        public ServiceBusSetting GetServiceBusSettings(string topicName)
        {
            return _serviceBusAccounts.ServiceBusSettings.First(x => x.Topic == topicName);
        }
    }
}