using System;
using System.Threading.Tasks;
using CloudAutomation.Application.Factories;
using CloudAutomation.Application.Interfaces.Smtp;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Models.Enums;
using CloudAutomation.Utils.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CloudAutomation.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmailController : Controller
    {
        private readonly IEmailFactory _emailFactory;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<EmailController> _logger;
        private readonly ISmtpService _smtpService;

        public EmailController(
            ISmtpService smtpService,
            IEmailFactory emailFactory,
            IUpdateStateClient updateStateClient,
            ILogger<EmailController> logger)
        {
            _smtpService = smtpService;
            _emailFactory = emailFactory;
            _updateStateClient = updateStateClient;
            _logger = logger;
        }

        [HttpPost("approval")]
        public async Task<IActionResult> SendApprovalEmail([FromBody] Email emailData)
        {
            try
            {
                var emailDetails = _emailFactory.GenerateEmailDetailsFromEmailData(emailData);
                await _smtpService.SendEmail(emailDetails);
                await _updateStateClient.Execute(emailData.Body.ResourceId, State.Approval);
                return Ok();
            }
            catch (Exception e)
            {
                await _updateStateClient.Execute(emailData.Body.ResourceId, State.Error);
                return Problem(e.Message);
            }
        }

        [HttpPost("created")]
        public async Task<IActionResult> SendCreationEmail([FromBody] CreatedResourceEmail emailData)
        {
            try
            {
                var emailDetails = _emailFactory.GenerateCreatedEmailFromEmailDataAndListOfValues(emailData.Email, emailData.Dictionary);
                await _smtpService.SendEmail(emailDetails);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }
    }
}