using CloudAutomation.Resources.StorageAccount.Processors.Interfaces;
using Microsoft.Extensions.Logging;
using Resource = CloudAutomation.Models.DevOps.Resource;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.StorageAccount.Processors
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