using System;
using System.Threading.Tasks;
using CloudAutomation.Application.Interfaces.Encryption;
using CloudAutomation.Application.Services;
using CloudAutomation.Models;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Models.Enums;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudAutomation.Controllers
{
    [Route("[controller]")]
    public class CreatedController : Controller
    {
        private readonly IEncryption _encryption;
        private readonly IGetWorkItem<Resource> _getWorkItem;
        private readonly IServiceBusSender _serviceBusSender;
        private readonly DevOpsSettings _devOpsSettings;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<CreatedController> _logger;
        
        private const string TopicName = "resource-approved";
        private const string RejectedTopicName = "resource-rejected";

        public CreatedController(
            IEncryption encryption,
            IGetWorkItem<Resource> getWorkItem,
            IOptions<DevOpsSettings> devOpsSettings,
            IServiceBusSender serviceBusSender,
            IUpdateStateClient updateStateClient,
            ILogger<CreatedController> logger)
        {
            _encryption = encryption;
            _getWorkItem = getWorkItem;
            _serviceBusSender = serviceBusSender;
            _devOpsSettings = devOpsSettings.Value;
            _updateStateClient = updateStateClient;
            _logger = logger;
        }
        
        [HttpGet("{status}")]
        public async Task<IActionResult> Index(string status)
        {
            WorkItemToken workItemToken;
            Resource workItem;
            try
            {
                workItemToken = JsonConvert.DeserializeObject<WorkItemToken>(_encryption.Decrypt(status));
                workItem = await _getWorkItem.GetWorkItemById(workItemToken.ResourceId, _devOpsSettings.Pat);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not deserialize work item token");
                throw;
            }

            var redirectUrl = GenerateRedirectUrl(workItem.Id.ToString());
            
            if (workItem.Fields.State != State.Resolved.ToString())
            {
                return Redirect(redirectUrl);
            }
            
            try
            {
                switch (workItemToken.Status)
                {
                    case Status.Approved:
                        await _updateStateClient.Execute(workItem.Id.ToString(), State.Done);
                        await _serviceBusSender.SendMessage(workItem, TopicName);
                        break;
                    case Status.Rejected:
                        await _updateStateClient.Execute(workItem.Id.ToString(), State.Rejected);
                        await _serviceBusSender.SendMessage(workItem, RejectedTopicName);
                        break;
                    default:
                        await _updateStateClient.Execute(workItem.Id.ToString(), State.Error);
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not change state");
                await _updateStateClient.Execute(workItem.Id.ToString(), State.Error);
            }

            return Redirect(redirectUrl);
        }

        private string GenerateRedirectUrl(string id)
        {
            return string.Format(_devOpsSettings.Url, id);
        }
    }
}