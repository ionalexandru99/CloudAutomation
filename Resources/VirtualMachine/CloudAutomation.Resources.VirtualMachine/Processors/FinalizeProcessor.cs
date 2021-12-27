using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;
using Microsoft.Extensions.Logging;
using Resource = CloudAutomation.Models.DevOps.Resource;

namespace CloudAutomation.Resources.VirtualMachine.Processors
{
    public class FinalizeProcessor : IFinalizeProcessor
    {
        private readonly ILogger<FinalizeProcessor> _logger;

        public FinalizeProcessor(ILogger<FinalizeProcessor> logger)
        {
            _logger = logger;
        }
        
        public void Execute(Resource resource)
        {
            _logger.LogInformation($"The resource with the id {resource.Id} has been created and approved");
        }
    }
}