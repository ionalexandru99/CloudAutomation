using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudAutomation.Application.Interfaces;
using CloudAutomation.Models;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAutomation.Application.Services
{
    public class WorkItemClient : IHttpClientRequest
    {
        private readonly DevOpsSettings _devOpsSettings;
        private readonly ILogger<WorkItemClient> _logger;

        public WorkItemClient(ILogger<WorkItemClient> logger, IOptions<DevOpsSettings> devOpsSettings)
        {
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
        }

        public async Task<bool> Execute(string url, string requestUrl)
        {
            try
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", _devOpsSettings.Pat.BasicEncoding());

                var workItemUri = new WorkItemUri(url);
                var json = JsonSerializer.Serialize(workItemUri);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, requestContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error appeared while trying to make request for work item");
                return false;
            }
        }
    }
}