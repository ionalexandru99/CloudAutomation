using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.VirtualMachine.Processors.Interfaces
{
    public interface IFinalizeProcessor
    {
        void Execute(Resource resource);
    }
}