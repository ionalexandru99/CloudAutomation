using Newtonsoft.Json;

namespace CloudAutomation.Resources.StorageAccount.Models
{
    public class Fields : CloudAutomation.Models.DevOps.Fields
    {
        [JsonProperty(PropertyName = "Custom.Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Custom.ResourceGroup")]
        public string ResourceGroup { get; set; }
    }
}