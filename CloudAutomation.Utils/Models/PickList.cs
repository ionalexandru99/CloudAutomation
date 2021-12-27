using System.Collections.Generic;

namespace CloudAutomation.Utils.Models
{
    public class PickList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsSuggested { get; set; }
        public string Url { get; set; }
        public List<string> Items { get; set; }
    }
}