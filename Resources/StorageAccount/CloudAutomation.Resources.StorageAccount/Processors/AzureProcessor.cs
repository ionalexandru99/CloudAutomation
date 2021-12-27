using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.StorageAccount.Models;
using CloudAutomation.Resources.StorageAccount.Processors.Interfaces;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.StorageAccount.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class, IStorageAccount
    {
        private readonly ServicePrincipalSettings _servicePrincipalSettings;
        private readonly ILogger<AzureProcessor<T>> _logger;

        private const string ResourceTagWorkItem = "WorkItem";
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
        
        public bool CreateResource(
            IAzure azure, 
            WorkItem resourceData, 
            IResourceGroup resourceGroup,
            out T resource) 
        {
            var resourceName = SdkContext.RandomResourceName("sa", 15);
            
            resource = null;
            try
            {
                resource = CreateStorageAccount(
                    azure, 
                    resourceData, 
                    resourceGroup, 
                    resourceName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the storage account with the name {resourceName}");

                RemoveResource(azure, resourceData, resourceGroup)
                    .GetAwaiter()
                    .GetResult();
                
                return false;
            }

            return true;
        }

        private T CreateStorageAccount(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string resourceName)
        {
            _logger.LogInformation(
                $"Creating storage account with the name {resourceName} for the work item with the id {resourceData.Id}");

            var resource = azure.StorageAccounts
                .Define(resourceName)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithAccessFromAllNetworks()
                .WithGeneralPurposeAccountKindV2()
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create() as T;
            
            _logger.LogInformation("Storage account created");
            return resource;
        }
        
        public async Task<bool> RemoveResource(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup)
        {
            try
            {
                _logger.LogInformation(
                    $"Deleting resources with the id {resourceData.Id} for the work item with the id {resourceData.Id}");

                var resources = await azure.GenericResources
                    .ListByTagAsync(
                        resourceGroup.Name,
                        ResourceTagWorkItemId,
                        resourceData.Id.ToString());

                foreach (var resource in resources)
                {
                    await azure.GenericResources.DeleteAsync(
                        resourceGroup.Name,
                        resource.ResourceProviderNamespace,
                        ResourceUtils.ParentRelativePathFromResourceId(resource.Id),
                        resource.ResourceType,
                        resource.Name,
                        resource.ApiVersion);
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to delete the resources with the id {resourceData.Id}");

                return false;
            }

            return true;
        }
        
        public IResourceGroup GetResourceGroup(IAzure azure, WorkItem resourceData)
        {
            var resourceGroupId = resourceData.Fields.ResourceGroup.Split("-").First().Trim();

            return azure.ResourceGroups
                .ListByTagAsync(ResourceTagWorkItemId, resourceGroupId)
                .GetAwaiter()
                .GetResult()
                .SingleOrDefault();
        }
    }
}