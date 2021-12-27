using CloudAutomation.Utils.Enums;
using CloudAutomation.Utils.Settings;

namespace CloudAutomation.Utils.Configurations
{
    public interface IServiceBusConfiguration
    {
        ServiceBusSetting GetServiceBusSettings(string topicName);
    }
}