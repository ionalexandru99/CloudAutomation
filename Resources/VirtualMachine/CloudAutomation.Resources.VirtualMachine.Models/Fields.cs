using Newtonsoft.Json;

namespace CloudAutomation.Resources.VirtualMachine.Models
{
    public class Fields : CloudAutomation.Models.DevOps.Fields
    {
        [JsonProperty(PropertyName = "Custom.Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Custom.ResourceGroup")]
        public string ResourceGroup { get; set; }
        
        [JsonProperty(PropertyName = "Custom.VirtualMachinesSizeTypes")] 
        public string VirtualMachinesSizeType { get; set; }
        
        [JsonProperty(PropertyName = "Custom.VirtualMachineImage")]
        public string VirtualMachineImage { get; set; }
    }
}