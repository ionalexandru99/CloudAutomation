using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudAutomation.Utils.Services
{
    public class GetWorkItem<T> : IGetWorkItem<T> where T : class
    {
        private readonly DevOpsSettings _devOpsSettings;

        public GetWorkItem(IOptions<DevOpsSettings> devOpsSettings)
        {
            _devOpsSettings = devOpsSettings.Value;
        }
        
        public async Task<T> GetWorkItemByUri(string url, string pat)
        {
            var client = GenerateHttpClient(pat);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException("Invalid arguments used for making the request");

            var json = await response.Content.ReadAsStringAsync();
            var workItem = JsonConvert.DeserializeObject<T>(json);

            return workItem;
        }

        private static HttpClient GenerateHttpClient(string pat)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat.BasicEncoding());
            return client;
        }

        public async Task<T> GetWorkItemById(string resourceId, string pat)
        {
            var client = GenerateHttpClient(pat);

            var url = GenerateUrlFromId(resourceId);
            
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException("Invalid arguments used for making the request");

            var json = await response.Content.ReadAsStringAsync();
            var workItem = JsonConvert.DeserializeObject<T>(json);

            return workItem;
        }

        private string GenerateUrlFromId(string resourceId)
        {
            var url = _devOpsSettings.GetWorkItemUri;
            return string.Format(url, resourceId);
        }
    }
}