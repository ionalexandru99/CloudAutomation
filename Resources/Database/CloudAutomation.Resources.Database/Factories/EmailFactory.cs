using System;
using System.Collections.Generic;
using System.Linq;
using CloudAutomation.Models.EmailTemplate;
using CloudAutomation.Resources.Database.Models;
using Microsoft.Azure.Management.Sql.Fluent;
using WorkItem = CloudAutomation.Resources.Database.Models.WorkItem;

namespace CloudAutomation.Resources.Database.Factories
{
    public class EmailFactory : IEmailFactory
    {
        public Email GetEmailFromResourceData(WorkItem workItem, bool getManager = true)
        {
            return new Email
            {
                Body = GenerateBodyFromResourceData(workItem),
                Recipient = GetRecipient(workItem, getManager),
                Subject = GenerateSubjectFromResourceData(workItem)
            };
        }
        
        public CreatedResourceEmail GetEmailDetailsFromResourceData(ISqlDatabase azureResource, WorkItem workItem, SqlServerCredentials credentials)
        {
            return new CreatedResourceEmail
            {
                Dictionary = GetResourceValues(azureResource, credentials),
                Email = GetEmailFromResourceData(workItem, false)
            };
        }

        private static Dictionary<string,string> GetResourceValues(ISqlDatabase azureResource, SqlServerCredentials credentials)
        {
            if (azureResource == null) throw new ArgumentNullException(nameof(azureResource));
            
            var dict =  new Dictionary<string, string>
            {
                {nameof(azureResource.Name), azureResource.Name},
                {nameof(azureResource.RegionName), azureResource.RegionName},
                {nameof(azureResource.SqlServerName), $"{azureResource.SqlServerName}.database.windows.net"},
                {"Username", credentials.Username},
                {"Password", credentials.Password}
            };
            
            var tags = azureResource.Tags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var (key, value) in tags)
            {
                dict.Add(key, value);
            }

            return dict;
        }

        private static Subject GenerateSubjectFromResourceData(WorkItem workItem)
        {
            return new Subject
            {
                ResourceType = workItem.Fields.WorkItemType,
                ResourceName = workItem.Fields.Name
            };
        }

        private static Body GenerateBodyFromResourceData(WorkItem workItem)
        {
            return new Body
            {
                ResourceId = workItem.Id.ToString(),
                ResourceType = workItem.Fields.WorkItemType
            };
        }

        private static string GetRecipient(WorkItem workItem, bool getManager = true)
        {
            return getManager ? 
                workItem.Fields.ResourceGroup.Split("-").Last().Trim() : 
                workItem.Fields.CreatedBy.UniqueName;
        }
    }
}