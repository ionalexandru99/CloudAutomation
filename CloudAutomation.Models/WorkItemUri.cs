using System;

namespace CloudAutomation.Models
{
    [Serializable]
    public class WorkItemUri
    {
        public WorkItemUri(string uri)
        {
            Uri = uri;
        }

        public WorkItemUri()
        {
            Uri = string.Empty;
        }

        public string Uri { get; set; }
    }
}