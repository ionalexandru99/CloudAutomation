using System;
using System.Collections.Generic;
using System.Text;

namespace CloudAutomation.Models.EmailTemplate
{
    public class CreatedResourceEmail
    {
        public Email Email { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
    }
}
