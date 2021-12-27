using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CloudAutomation.Models.Enums;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Utils.Services
{
    public class UpdateStateClient : IUpdateStateClient
    {
        private readonly ILogger<UpdateStateClient> _logger;
        private readonly DevOpsSettings _devOpsSettings;

        private const string RequestBody = "[{\"op\": \"add\",\"path\": \"/fields/System.State\",\"value\": \"{0}\"}]";
        private const string ReplaceString = "{0}";

        public UpdateStateClient(IOptions<DevOpsSettings> devOpsSettings, ILogger<UpdateStateClient> logger)
        {
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
        }

        public async Task<bool> Execute(string resourceId, State state)
        {
            try
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", _devOpsSettings.Pat.BasicEncoding());

                var uri = GenerateUri(resourceId);
                var json = GenerateBody(state);

                var requestContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");

                var response = await client.PatchAsync(uri, requestContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            
            catch (Exception e)
            {
                _logger.LogError(e, "The update status was not successful");
                return false;
            }
        }

        private string GenerateBody(State state)
        {
            var stateString = state.ToExtendedString();

            return RequestBody.Replace(ReplaceString, stateString);
        }

        private string GenerateUri(string resourceId)
        {
            return string.Format(_devOpsSettings.UpdateWorkItemUri, resourceId);
        }
    }
}