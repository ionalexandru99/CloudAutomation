using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Application.Interfaces;
using CloudAutomation.DataAccess.Repository;
using CloudAutomation.Models;
using CloudAutomation.Models.Enums;
using CloudAutomation.Models.ServiceBus;
using CloudAutomation.Utils.Services;

namespace CloudAutomation.Application.Processors
{
    public class WorkItemProcessor : IWorkItemProcessor
    {
        private readonly IHttpClientRequest _request;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ICustomServiceProcessor _customServiceProcessor;
        private readonly IWorkItemRepository _workItemRepository;

        public WorkItemProcessor(
            IWorkItemRepository workItemRepository,
            IHttpClientRequest request,
            IUpdateStateClient updateStateClient,
            ICustomServiceProcessor customServiceProcessor)
        {
            _workItemRepository = workItemRepository;
            _request = request;
            _updateStateClient = updateStateClient;
            _customServiceProcessor = customServiceProcessor;
        }

        public async Task Execute(WorkItem workItem)
        {
            var workItems = await _workItemRepository.GetAll();
            var workItemRepo = workItems.SingleOrDefault(
                w => string.Equals(
                    w.ResourceType.Trim(),
                    workItem.Resource.Fields.WorkItemType.Trim(), 
                    StringComparison.CurrentCultureIgnoreCase));

            if (workItemRepo == default || !workItemRepo.Active)
            {
                await _customServiceProcessor.Execute(workItem);
                return;
            }

            var requestResult = await _request.Execute(workItem.Resource.Url, workItemRepo.RequestUrl);

            if (requestResult) return;

            requestResult = await _updateStateClient.Execute(workItem.Resource.Id.ToString(), State.Error);

            if (!requestResult)
                throw new Exception("An error appeared and the state could not be changed");
        }
    }
}