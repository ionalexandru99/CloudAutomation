using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.WebApplication.Processors.Interfaces
{
    public interface IFinalizeProcessor
    {
        void Execute(Resource resource);
    }
}