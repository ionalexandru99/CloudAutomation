using CloudAutomation.Models.Enums;

namespace CloudAutomation.Models
{
    public class WorkItemToken
    {
        public string ResourceId { get; set; }
        public Status Status { get; set; }
    }
}