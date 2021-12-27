using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Models;
using CloudAutomation.Models.Enums;
using CloudAutomation.Resources.ResourceGroup.Clients;
using CloudAutomation.Resources.ResourceGroup.Factories;
using CloudAutomation.Resources.ResourceGroup.Models;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResourceGroupWorkItem = CloudAutomation.Resources.ResourceGroup.Models.WorkItem;

namespace CloudAutomation.Resources.ResourceGroup.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResourceGroupController : Controller
    {
        private const string PatHeader = "Authorization";
        private readonly IGetWorkItem<WorkItem> _getWorkItem;
        private readonly IEmailFactory _emailFactory;
        private readonly IEmailClient _emailClient;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<ResourceGroupController> _logger;

        public ResourceGroupController(
            IGetWorkItem<WorkItem> getWorkItem,
            IEmailFactory emailFactory,
            IEmailClient emailClient,
            IUpdateStateClient updateStateClient,
            ILogger<ResourceGroupController> logger)
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

            WorkItem azureResourceGroup;

            try
            {
                azureResourceGroup = await _getWorkItem.GetWorkItemByUri(workItemUrl.Uri, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to get work item information from DevOps");
                return Problem();
            }

            try
            {
                var email = _emailFactory.GetEmailFromResourceData(azureResourceGroup);

                var emailSent = await _emailClient.SendResourceForApproval(email);

                if (emailSent)
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to send email for approving resource");
            }
            await _updateStateClient.Execute(azureResourceGroup.Id.ToString(), State.Error);
            return BadRequest("Could not send email");
        }
    }
}