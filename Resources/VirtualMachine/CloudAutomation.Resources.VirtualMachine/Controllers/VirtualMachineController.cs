﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Models;
using CloudAutomation.Models.Enums;
using CloudAutomation.Resources.VirtualMachine.Clients;
using CloudAutomation.Resources.VirtualMachine.Factories;
using CloudAutomation.Resources.VirtualMachine.Models;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VirtualMachineWorkItem = CloudAutomation.Resources.VirtualMachine.Models.WorkItem;

namespace CloudAutomation.Resources.VirtualMachine.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VirtualMachineController : Controller
    {
        private const string PatHeader = "Authorization";
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly IEmailFactory _emailFactory;
        private readonly IEmailClient _emailClient;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<VirtualMachineController> _logger;

        public VirtualMachineController(
            IGetWorkItem<WorkItem> getWorkItem,
            IEmailFactory emailFactory,
            IEmailClient emailClient,
            IUpdateStateClient updateStateClient,
            ILogger<VirtualMachineController> logger)
        {
            _getWorkItem = getWorkItem;
            _emailFactory = emailFactory;
            _emailClient = emailClient;
            _updateStateClient = updateStateClient;
            _logger = logger;
        }

        [HttpPost("approval")]
        public async Task<IActionResult> SendToApproval([FromBody] WorkItemUri workItemUrl)
        {
            var request = Request;
            var headers = request.Headers;

            if (!headers.ContainsKey(PatHeader)) return Unauthorized();
            var token = headers[PatHeader].SingleOrDefault()?.BasicDecoding();

            var azureResource = await GetWorkItemData(workItemUrl.Uri, token);

            if (azureResource == null) return Problem();
            
            var isEmailSend = await SendEmail(azureResource);
            if (isEmailSend) return Ok();
            
            await _updateStateClient.Execute(azureResource.Id.ToString(), State.Error);
            return BadRequest("Could not send email");
        }


        private async Task<WorkItem> GetWorkItemData(string url, string token)
        {
            try
            {
                return await _getWorkItem.GetWorkItemByUri(url, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to get work item information from DevOps");
                return null;
            }
        }

        private async Task<bool> SendEmail(WorkItem azureResource)
        {
            try
            {
                var email = _emailFactory.GetEmailFromResourceData(azureResource);

                var emailSent = await _emailClient.SendResourceForApproval(email);

                if (emailSent) return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to send email for approving resource");
            }

            return false;
        }
    }
}