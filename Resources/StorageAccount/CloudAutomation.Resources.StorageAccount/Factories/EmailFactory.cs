using System;
using System.Collections.Generic;
using System.Linq;
using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.Storage.Fluent;
using WorkItem = CloudAutomation.Resources.StorageAccount.Models.WorkItem;

namespace CloudAutomation.Resources.StorageAccount.Factories
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
        
        public CreatedResourceEmail GetEmailDetailsFromResourceData(IStorageAccount azureResource, WorkItem workItem)
        {
            return new CreatedResourceEmail
            {
                Dictionary = GetResourceValues(azureResource),
                Email = GetEmailFromResourceData(workItem, false)
            };
        }

        private static Dictionary<string,string> GetResourceValues(IStorageAccount azureResource)
        {
            if (azureResource == null) throw new ArgumentNullException(nameof(azureResource));

            var storageKeys = azureResource.GetKeys();
            var storageConnectionString = "DefaultEndpointsProtocol=https;"
                                          + "AccountName=" + azureResource.Name
                                          + ";AccountKey=" + storageKeys[0].Value
                                          + ";EndpointSuffix=core.windows.net";

            
            var dict =  new Dictionary<string, string>
            {
                {nameof(azureResource.Name), azureResource.Name},
                {nameof(azureResource.RegionName), azureResource.RegionName},
                {"ConnectionString", storageConnectionString}
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