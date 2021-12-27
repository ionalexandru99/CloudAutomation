using System.Collections.Generic;
using System.Text;
using CloudAutomation.Application.Interfaces.Encryption;
using CloudAutomation.Application.Interfaces.File;
using CloudAutomation.Models;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Models.Enums;
using CloudAutomation.Utils.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CloudAutomation.Application.Factories
{
    public class EmailFactory : IEmailFactory
    {
        private const string Subject = "Cloud Automation - {0} - {1}";
        private const string HtmlFilePath = "CloudAutomation.Application.Resources.EmailTemplate.html";
        private const string CreatedResourceHtmlFilePath = "CloudAutomation.Application.Resources.CreatedResource.html";
        private const string RowDetails = "CloudAutomation.Application.Resources.RowDetails.html";

        private const string ReplaceFirstValue = "{0}";
        private const string ReplaceSecondValue = "{1}";
        private const string ReplaceThirdValue = "{2}";
        private const string ReplaceForthValue = "{3}";
        
        private readonly EmailSettings _approvalSettings;
        private readonly DevOpsSettings _devOpsSettings;
        private readonly IReadFile _emailTemplateReader;
        private readonly IEncryption _encryption;
        
        public EmailFactory(IReadFile emailTemplateReader, IOptions<DevOpsSettings> devOpsSettings,
            IEncryption encryption, IOptions<EmailSettings> approvalSettings)
        {
            _emailTemplateReader = emailTemplateReader;
            _encryption = encryption;
            _approvalSettings = approvalSettings.Value;
            _devOpsSettings = devOpsSettings.Value;
        }

        public EmailDetails GenerateEmailDetailsFromEmailData(Email emailData)
        {
            var emailDetails = new EmailDetails
            {
                Subject = CreateSubject(emailData.Subject),
                Body = GenerateBody(emailData.Body),
                Recipient = emailData.Recipient
            };

            return emailDetails;
        }

        public EmailDetails GenerateCreatedEmailFromEmailDataAndListOfValues(Email emailData,
            Dictionary<string, string> dictionary)
        {
            var emailDetails = new EmailDetails
            {
                Subject = CreateSubject(emailData.Subject),
                Body = GenerateBodyWithDetails(emailData.Body, dictionary),
                Recipient = emailData.Recipient
            };

            return emailDetails;
        }

        private string GenerateBodyWithDetails(Body body, Dictionary<string, string> dictionary)
        {
            var htmlBody = _emailTemplateReader.Execute(CreatedResourceHtmlFilePath);
            var row = _emailTemplateReader.Execute(RowDetails);

            var rows = GenerateRows(row, dictionary);
            var approvalUri = GenerateWorkItemToken(body.ResourceId, Status.Approved, _approvalSettings.CreatedUri);
            var rejectUri = GenerateWorkItemToken(body.ResourceId, Status.Rejected, _approvalSettings.CreatedUri);

            htmlBody = htmlBody.Replace(ReplaceFirstValue, rows);
            htmlBody = htmlBody.Replace(ReplaceSecondValue, approvalUri);
            htmlBody = htmlBody.Replace(ReplaceThirdValue, rejectUri);
            
            return htmlBody;
        }

        private string GenerateRows(string rowTemplate, Dictionary<string, string> dictionary)
        {
            var rows = new StringBuilder(string.Empty);
            foreach (var pair in dictionary)
            {
                var tableRow = string.Format(rowTemplate, pair.Key, pair.Value);
                rows.Append(tableRow);
            }

            return rows.ToString();
        }

        private string GenerateBody(Body emailDataBody)
        {
            var htmlBody = _emailTemplateReader.Execute(HtmlFilePath);

            var detailsUri = string.Format(_devOpsSettings.Url, emailDataBody.ResourceId);
            var approvalUri = GenerateWorkItemToken(emailDataBody.ResourceId, Status.Approved, _approvalSettings.ApprovalUri);
            var rejectUri = GenerateWorkItemToken(emailDataBody.ResourceId, Status.Rejected, _approvalSettings.ApprovalUri);

            htmlBody = htmlBody.Replace(ReplaceFirstValue, emailDataBody.ResourceType);
            htmlBody = htmlBody.Replace(ReplaceSecondValue, detailsUri);
            htmlBody = htmlBody.Replace(ReplaceThirdValue, approvalUri);
            htmlBody = htmlBody.Replace(ReplaceForthValue, rejectUri);

            return htmlBody;
        }

        private string GenerateWorkItemToken(string resourceId, Status status, string link)
        {
            var token = new WorkItemToken
            {
                ResourceId = resourceId,
                Status = status
            };

            var serializedToken = JsonConvert.SerializeObject(token);
            var encryptedToken = _encryption.Encrypt(serializedToken);

            link = string.Format(link, encryptedToken);

            return link;
        }

        private static string CreateSubject(Subject subject)
        {
            var emailSubject = string.Format(Subject, subject.ResourceType, subject.ResourceName);

            return emailSubject;
        }
    }
}