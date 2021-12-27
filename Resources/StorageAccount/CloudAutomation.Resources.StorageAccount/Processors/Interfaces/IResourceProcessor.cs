using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.StorageAccount.Processors.Interfaces
{
    public interface IResourceProcessor<T> where T : class
    {
        Task<T> Execute(Resource resource);
    }
}
