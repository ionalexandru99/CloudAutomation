using Newtonsoft.Json;

namespace CloudAutomation.Models.DevOps
{
    public class Fields
    {
        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string WorkItemType { get; set; }

        [JsonProperty(PropertyName = "System.State")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "System.BoardColumn")]
        public string BoardColumn { get; set; }

        [JsonProperty(PropertyName = "System.CreatedBy")]
        public CreatedBy CreatedBy { get; set; }
    }
}