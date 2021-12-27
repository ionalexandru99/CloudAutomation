using Newtonsoft.Json;

namespace CloudAutomation.Resources.ResourceGroup.Models
{
    public class Fields : CloudAutomation.Models.DevOps.Fields
    {
        [JsonProperty(PropertyName = "Custom.Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Custom.Location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "Custom.Manager")]
        public string Manager { get; set; }
    }
}