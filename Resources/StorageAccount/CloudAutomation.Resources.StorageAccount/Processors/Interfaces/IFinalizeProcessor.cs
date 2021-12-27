using CloudAutomation.Models.DevOps;

namespace CloudAutomation.Resources.StorageAccount.Processors.Interfaces
{
    public interface IFinalizeProcessor
    {
        void Execute(Resource resource);
    }
}