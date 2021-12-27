using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Resources.Database.Models;

namespace CloudAutomation.Resources.Database.Processors.Interfaces
{
    public interface IResourceProcessor<T> where T : class
    {
        T Execute(Resource resource, out SqlServerCredentials credentials);
    }
}
