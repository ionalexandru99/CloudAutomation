using System;

namespace CloudAutomation.Models.ServiceBus
{
    [Serializable]
    public class WorkItem
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public Resource Resource { get; set; }
        public string CreatedDate { get; set; }
    }
}