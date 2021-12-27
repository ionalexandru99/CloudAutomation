using System.Threading.Tasks;
using CloudAutomation.Models.ServiceBus;

namespace CloudAutomation.Application.Interfaces
{
    public interface ICustomServiceProcessor
    {
        Task Execute(WorkItem item);
    }
}