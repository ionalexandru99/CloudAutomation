using Newtonsoft.Json;

namespace CloudAutomation.Resources.WebApplication.Models
{
    public class Fields : CloudAutomation.Models.DevOps.Fields
    {
        [JsonProperty(PropertyName = "Custom.Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Custom.ResourceGroup")]
        public string ResourceGroup { get; set; }

        [JsonProperty(PropertyName = "Custom.RuntimeStack")]
        public string RuntimeStack { get; set; }
        
        [JsonProperty(PropertyName = "Custom.PricingTier")]
        public string PricingTier { get; set; }
    }
}