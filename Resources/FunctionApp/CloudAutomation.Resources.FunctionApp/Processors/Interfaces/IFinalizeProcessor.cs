using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.FunctionApp.Processors.Interfaces
{
    public interface IFinalizeProcessor
    {
        void Execute(Resource resource);
    }
}