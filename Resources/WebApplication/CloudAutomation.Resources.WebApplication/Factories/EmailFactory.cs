using System;
using System.Collections.Generic;
using System.Linq;
using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using WorkItem = CloudAutomation.Resources.WebApplication.Models.WorkItem;

namespace CloudAutomation.Resources.WebApplication.Factories
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
        
        public CreatedResourceEmail GetEmailDetailsFromResourceData(IWebApp azureResource, WorkItem workItem)
        {
            return new CreatedResourceEmail
            {
                Dictionary = GetResourceValues(azureResource),
                Email = GetEmailFromResourceData(workItem, false)
            };
        }

        private static Dictionary<string,string> GetResourceValues(IWebApp azureResource)
        {
            if (azureResource == null) throw new ArgumentNullException(nameof(azureResource));

            var dict =  new Dictionary<string, string>
            {
                {nameof(azureResource.Name), azureResource.Name},
                {nameof(azureResource.RegionName), azureResource.RegionName},
                {nameof(azureResource.DefaultHostName), azureResource.DefaultHostName},
                {nameof(azureResource.OperatingSystem), azureResource.OperatingSystem.ToString()}
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