namespace CloudAutomation.DataAccess.Models
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string ResourceType { get; set; }
        public string RequestUrl { get; set; }
        public bool Active { get; set; }
    }
}