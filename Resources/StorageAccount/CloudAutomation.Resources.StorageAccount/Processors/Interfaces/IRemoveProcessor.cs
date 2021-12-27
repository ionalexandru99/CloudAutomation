using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.StorageAccount.Processors.Interfaces
{
    public interface IRemoveProcessor
    {
        Task Execute(Resource resource);
    }
}