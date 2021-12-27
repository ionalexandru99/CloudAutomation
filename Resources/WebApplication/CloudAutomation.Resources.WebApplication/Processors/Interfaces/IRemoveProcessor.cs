using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.WebApplication.Processors.Interfaces
{
    public interface IRemoveProcessor
    {
        Task Execute(Resource resource);
    }
}