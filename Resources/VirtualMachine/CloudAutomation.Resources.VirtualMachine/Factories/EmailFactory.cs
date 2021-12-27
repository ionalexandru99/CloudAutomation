using System.Collections.Generic;
using System.Linq;
using CloudAutomation.Models.EmailTemplate;
using Microsoft.Azure.Management.Compute.Fluent;
using WorkItem = CloudAutomation.Resources.VirtualMachine.Models.WorkItem;

namespace CloudAutomation.Resources.VirtualMachine.Factories
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

        public CreatedResourceEmail GetEmailDetailsFromResourceData(IVirtualMachine azureResource, WorkItem workItem, string username, string password)
        {
            return new CreatedResourceEmail
            {
                Dictionary = GetResourceValues(azureResource, username, password),
                Email = GetEmailFromResourceData(workItem, false)
            };
        }

        //This is the function that needs to be modified for each microservice
        private static Dictionary<string,string> GetResourceValues(IVirtualMachine azureResource, string username, string password)
        {
            var dict =  new Dictionary<string, string>
            {
                {nameof(azureResource.Name), azureResource.Name},
                {nameof(azureResource.RegionName), azureResource.RegionName},
                {nameof(azureResource.Size), azureResource.Size.ToString()},
                {nameof(azureResource.OSType), azureResource.OSType.ToString()},
                {"Public IP", azureResource.GetPrimaryPublicIPAddress().IPAddress},
                {"Username", username},
                {"Password", password}
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