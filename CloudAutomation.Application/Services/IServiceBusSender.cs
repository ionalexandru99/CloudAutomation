using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Application.Services
{
    public interface IServiceBusSender
    {
        Task SendMessage(Resource workItem, string topicName);
    }
}
