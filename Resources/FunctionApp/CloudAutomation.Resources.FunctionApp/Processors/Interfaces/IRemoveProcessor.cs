using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.FunctionApp.Processors.Interfaces
{
    public interface IRemoveProcessor
    {
        Task Execute(Resource resource);
    }
}