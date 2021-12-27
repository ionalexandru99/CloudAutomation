using System;
using System.Threading.Tasks;
using CloudAutomation.Application.Factories;
using CloudAutomation.Application.Interfaces;
using CloudAutomation.Application.Interfaces.Smtp;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Models.Enums;
using CloudAutomation.Models.ServiceBus;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Application.Implementations
{
    public class CustomServiceProcessor : ICustomServiceProcessor
    {
        private readonly ISmtpService _smtpService;
        private readonly IEmailFactory _emailFactory;
        private readonly IUpdateStateClient _updateStateClient;
        private readonly ILogger<CustomServiceProcessor> _logger;
        private readonly CloudTeamSettings _cloudTeamOptions;

        public CustomServiceProcessor(
            ISmtpService smtpService,
            IEmailFactory emailFactory,
            IUpdateStateClient updateStateClient,
            ILogger<CustomServiceProcessor> logger,
            IOptions<CloudTeamSettings> cloudTeamOptions)
        {
            _smtpService = smtpService;
            _emailFactory = emailFactory;
            _updateStateClient = updateStateClient;
            _logger = logger;
            _cloudTeamOptions = cloudTeamOptions.Value;
        }
        
        public async Task Execute(WorkItem item)
        {
            var emailData = GenerateDefaultEmailDetails(item);
            
            try
            {
                var emailDetails = _emailFactory.GenerateEmailDetailsFromEmailData(emailData);
                await _smtpService.SendEmail(emailDetails);
                await _updateStateClient.Execute(emailData.Body.ResourceId, State.Approval);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send email to cloud team");
                await _updateStateClient.Execute(emailData.Body.ResourceId, State.Error);
            }
        }

        private Email GenerateDefaultEmailDetails(WorkItem item)
        {
            return new Email
            {
                Subject = new Subject
                {
                    ResourceName = item.Resource.Fields.Title,
                    ResourceType = item.Resource.Fields.WorkItemType
                },
                Body = new Body
                {
                    ResourceId = item.Resource.Id.ToString(),
                    ResourceType = item.Resource.Fields.WorkItemType
                },
                Recipient = _cloudTeamOptions.Email
            };
        }
    }
}