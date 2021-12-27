namespace CloudAutomation.Models.EmailTemplate
{
    public class Email
    {
        public Subject Subject { get; set; }
        public Body Body { get; set; }
        public string Recipient { get; set; }
    }
}