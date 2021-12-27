using System.Threading.Tasks;
using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.ResourceGroup.Processors.Interfaces
{
    public interface IFinalizeProcessor
    {
        Task Execute(Resource resource);
    }
}