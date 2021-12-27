using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.ResourceGroup.Models;
using CloudAutomation.Resources.ResourceGroup.Processors.Interfaces;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.ResourceGroup.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class
    {
        private readonly ServicePrincipalSettings _servicePrincipalSettings;
        private readonly ILogger<AzureProcessor<T>> _logger;

        private const string ResourceTagWorkItem = "WorkItem";
        private const string ResourceTagGroupManager = "Manager";
        private const string ResourceTagWorkItemId = "Id";
        
        public AzureProcessor(
            IOptions<ServicePrincipalSettings> servicePrincipalSettings,
            ILogger<AzureProcessor<T>> logger)
        {
            _servicePrincipalSettings = servicePrincipalSettings.Value;
            _logger = logger;
        }
        
        public IAzure LoginToAzure()
        {
            _logger.LogInformation("Connection to Azure");
            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(_servicePrincipalSettings.Client,
                    _servicePrincipalSettings.Key,
                    _servicePrincipalSettings.Tenant,
                    AzureEnvironment.AzureGlobalCloud);

            var azure = Azure
                .Configure()
                .Authenticate(credentials)
                .WithSubscription(_servicePrincipalSettings.Subscription);
            return azure;
        }

        public bool CreateResource(IAzure azure, WorkItem resourceData, out T resource)
        {
            var resourceGroupName = SdkContext.RandomResourceName("rg-", 24);
            
            resource = null;
            
            try
            {
                _logger.LogInformation($"Creation resource group with the name {resourceGroupName} for the work item with the name {resourceData.Fields.Name}");
                resource = azure.ResourceGroups
                    .Define(resourceGroupName)
                    .WithRegion(Region.Create(resourceData.Fields.Location))
                    .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                    .WithTag(ResourceTagGroupManager, resourceData.Fields.Manager)
                    .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                    .Create() as T;

                _logger.LogInformation("Resource Group created");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the resource group with the name {resourceGroupName}");

                return false;
            }

            return true;
        }

        public async Task<bool> RemoveResource(IAzure azure, WorkItem resourceData)
        {
            try
            {
                _logger.LogInformation(
                    $"Deleting resource group with the id {resourceData.Id} for the work item with the name {resourceData.Fields.Name}");

                var resourceGroup = azure.ResourceGroups
                    .ListByTagAsync(ResourceTagWorkItemId, resourceData.Id.ToString())
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault();

                if (resourceGroup != null)
                    await azure.ResourceGroups
                        .DeleteByNameAsync(resourceGroup.Name);
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to delete the resource group with the id {resourceData.Id}");

                return false;
            }

            return true;
        }
    }
}