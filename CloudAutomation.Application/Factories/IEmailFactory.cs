using System.Collections.Generic;
using CloudAutomation.Models.EmailTemplate;

namespace CloudAutomation.Application.Factories
{
    public interface IEmailFactory
    {
        EmailDetails GenerateEmailDetailsFromEmailData(Email emailData);

        EmailDetails GenerateCreatedEmailFromEmailDataAndListOfValues(Email emailData,
            Dictionary<string, string> dictionary);
    }
}