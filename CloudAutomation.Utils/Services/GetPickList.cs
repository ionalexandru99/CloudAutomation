using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudAutomation.Utils.Extensions;
using CloudAutomation.Utils.Models;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudAutomation.Utils.Services
{
    public class GetPickList : IGetPickList
    {
        private readonly DevOpsSettings _devOpsSettings;

        public GetPickList(IOptions<DevOpsSettings> devOpsSettings)
        {
            _devOpsSettings = devOpsSettings.Value;
        }
        
        public async Task<PickList> Execute(string id)
        {
            var client = GenerateHttpClient(_devOpsSettings.Pat);

            var url = GenerateUrlFromId(id);
            
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException("Invalid arguments used for making the request");

            var json = await response.Content.ReadAsStringAsync();
            var pickList = JsonConvert.DeserializeObject<PickList>(json);

            return pickList;
        }
        
        private static HttpClient GenerateHttpClient(string pat)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat.BasicEncoding());
            return client;
        }
        
        private string GenerateUrlFromId(string id)
        {
            var url = _devOpsSettings.GetPickList;
            return string.Format(url, id);
        }
    }
}