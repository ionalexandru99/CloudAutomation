﻿using System;
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
    public class ApprovalController : Controller
    {
        private readonly IEncryption _encryption;
        private readonly IGetWorkItem<Resource> _getWorkItem;
        private readonly IServiceBusSender _serviceBusSender;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<ApprovalController> _logger;
        private readonly DevOpsSettings _devOpsSettings;
        
        private const string TopicName = "resource-creation";

        public ApprovalController(
            IEncryption encryption, 
            IGetWorkItem<Resource> getWorkItem, 
            IOptions<DevOpsSettings> devOpsSettings,
            IServiceBusSender serviceBusSender,
            IUpdateStateClient updateStateClient,
            ILogger<ApprovalController> logger)
        {
            _encryption = encryption;
            _getWorkItem = getWorkItem;
            _serviceBusSender = serviceBusSender;
            _updateStateClient = updateStateClient;
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
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

            if (workItem.Fields.State != State.Approval.ToString())
            {
                return Redirect(redirectUrl);
            }

            try
            {
                switch (workItemToken.Status)
                {
                    case Status.Approved:
                        await _updateStateClient.Execute(workItem.Id.ToString(), State.Approved);
                        await _serviceBusSender.SendMessage(workItem, TopicName);
                        break;
                    case Status.Rejected:
                        await _updateStateClient.Execute(workItem.Id.ToString(), State.Rejected);
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