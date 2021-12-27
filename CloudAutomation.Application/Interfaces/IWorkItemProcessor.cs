using System.Threading.Tasks;
using CloudAutomation.Models;
using CloudAutomation.Models.ServiceBus;

namespace CloudAutomation.Application.Interfaces
{
    public interface IWorkItemProcessor
    {
        public Task Execute(WorkItem workItem);
    }
}