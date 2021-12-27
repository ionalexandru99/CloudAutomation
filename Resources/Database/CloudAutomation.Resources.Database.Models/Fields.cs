using Newtonsoft.Json;

namespace CloudAutomation.Resources.Database.Models
{
    public class Fields : CloudAutomation.Models.DevOps.Fields
    {
        [JsonProperty(PropertyName = "Custom.Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Custom.ResourceGroup")]
        public string ResourceGroup { get; set; }

        [JsonProperty(PropertyName = "Custom.DatabaseSize")]
        public string Size { get; set; }

        [JsonProperty(PropertyName = "Custom.ServerName")]
        public string ServerName { get; set; }
    }
}