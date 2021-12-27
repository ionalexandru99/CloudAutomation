using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Resources.Database.Clients
{
    public class EmailClient : IEmailClient
    {
        private readonly ILogger<EmailClient> _logger;
        private readonly ApprovalSettings _approvalSettings;

        public EmailClient(IOptions<ApprovalSettings> approvalSettings, ILogger<EmailClient> logger)
        {
            _logger = logger;
            _approvalSettings = approvalSettings.Value;
        }

        public async Task<bool> SendResourceForApproval(Email email)
        {
            try
            {
                var client = new HttpClient();
                
                var json = JsonSerializer.Serialize(email);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_approvalSettings.RequestUri, requestContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to send request with email data");
                return false;
            }
        }

        public async Task<bool> SendResourceForConfirmation(CreatedResourceEmail createdResourceEmail)
        {
            try
            {
                var client = new HttpClient();
                
                var json = JsonSerializer.Serialize(createdResourceEmail);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_approvalSettings.ConfirmUri, requestContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to send confirmation with email data");
                return false;
            }
        }
    }
}