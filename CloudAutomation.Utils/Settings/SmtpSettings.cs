using System.Net.Mail;

namespace CloudAutomation.Utils.Settings
{
    public static class SmtpSettings
    {
        public const bool IsBodyHtml = true;
        public const int Port = 587;
        public const string Host = "smtp.gmail.com";
        public const bool EnableSsl = true;
        public const bool UseDefaultCredentials = false;
        public const SmtpDeliveryMethod DeliveryMethod = SmtpDeliveryMethod.Network;
    }
}