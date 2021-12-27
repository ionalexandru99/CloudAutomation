using System.Collections.Generic;
using System.Linq;
using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using WorkItem = CloudAutomation.Resources.ResourceGroup.Models.WorkItem;

namespace CloudAutomation.Resources.ResourceGroup.Factories
{
    public class EmailFactory : IEmailFactory
    {
        public Email GetEmailFromResourceData(WorkItem resourceGroup, bool getManager = true)
        {
            return new Email
            {
                Body = GenerateBodyFromResourceData(resourceGroup),
                Recipient = GetRecipient(resourceGroup, getManager),
                Subject = GenerateSubjectFromResourceData(resourceGroup)
            };
        }

        public CreatedResourceEmail GetEmailDetailsFromResourceData(IResourceGroup azureResource, WorkItem workItem)
        {
            return new CreatedResourceEmail
            {
                Dictionary = GetResourceValues(azureResource),
                Email = GetEmailFromResourceData(workItem, false)
            };
        }

        //This is the function that needs to be modified for each microservice
        private Dictionary<string,string> GetResourceValues(IResourceGroup azureResource)
        {
            var dict =  new Dictionary<string, string>
            {
                {nameof(azureResource.Name), azureResource.Name},
                {nameof(azureResource.RegionName), azureResource.RegionName}
            };
            var tags = azureResource.Tags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var (key, value) in tags)
            {
                dict.Add(key, value);
            }

            return dict;
        }

        private Subject GenerateSubjectFromResourceData(WorkItem resourceGroup)
        {
            return new Subject
            {
                ResourceType = resourceGroup.Fields.WorkItemType,
                ResourceName = resourceGroup.Fields.Name
            };
        }

        private Body GenerateBodyFromResourceData(WorkItem resourceGroup)
        {
            return new Body
            {
                ResourceId = resourceGroup.Id.ToString(),
                ResourceType = resourceGroup.Fields.WorkItemType
            };
        }

        private string GetRecipient(WorkItem workItem, bool getManager = true)
        {
            if (getManager)
            {
                return workItem.Fields.Manager;
            }

            return workItem.Fields.CreatedBy.UniqueName;
        }
    }
}