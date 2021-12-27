using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Models;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudAutomation.Utils.Services
{
    public class UpdatePickList : IUpdatePickList
    {
        private readonly ILogger<UpdatePickList> _logger;
        private readonly DevOpsSettings _devOpsSettings;

        public UpdatePickList(IOptions<DevOpsSettings> devOpsSettings, ILogger<UpdatePickList> logger)
        {
            _logger = logger;
            _devOpsSettings = devOpsSettings.Value;
        }
        
        public async Task<bool> Execute(PickList pickList)
        {
            try
            {
                var client = GenerateHttpClient(_devOpsSettings.Pat);

                var uri = GenerateUrlFromId(pickList.Id);
                var json = JsonConvert.SerializeObject(pickList);

                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(uri, requestContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The update status was not successful");
                return false;
            }
        }
        
        private static HttpClient GenerateHttpClient(string pat)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat.BasicEncoding());
            return client;
        }
        
        private string GenerateUrlFromId(string id)
        {
            var url = _devOpsSettings.UpdatePickList;
            return string.Format(url, id);
        }
    }
}