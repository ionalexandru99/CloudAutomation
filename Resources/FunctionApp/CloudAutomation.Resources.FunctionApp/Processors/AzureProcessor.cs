using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.FunctionApp.Models;
using CloudAutomation.Resources.FunctionApp.Processors.Interfaces;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OperatingSystem = Microsoft.Azure.Management.AppService.Fluent.OperatingSystem;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.FunctionApp.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class, IFunctionApp
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
            // ReSharper disable twice StringLiteralTypo
            var resourceName = SdkContext.RandomResourceName("cloudfunc", 15);
            var appServiceName = SdkContext.RandomResourceName("appservice", 15);
            var storageAccountName = SdkContext.RandomResourceName("sa", 15);
            
            resource = null;
            try
            {
                var appService = CreateAppService(azure, resourceData, resourceGroup, appServiceName);
                var storageAccount = CreateStorageAccount(azure, resourceData, resourceGroup, storageAccountName);
                
                resource = CreateFunctionApp(
                    azure, 
                    resourceData, 
                    resourceGroup, 
                    appService,
                    storageAccount,
                    resourceName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the function app for the work item with the id {resourceData.Id}");

                RemoveResource(azure, resourceData, resourceGroup)
                    .GetAwaiter()
                    .GetResult();
                
                return false;
            }

            return true;
        }

        private IAppServicePlan CreateAppService(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string appServiceName)
        {
            _logger.LogInformation(
                $"Creating app service with the name {appServiceName} for the work item with the id {resourceData.Id}");
            
            var resource = azure.AppServices.AppServicePlans
                .Define(appServiceName)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithPricingTier(
                    (PricingTier) typeof(PricingTier).GetField(resourceData.Fields.PricingTier)?.GetValue(null))
                .WithOperatingSystem(OperatingSystem.Windows)
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create();
            
            _logger.LogInformation("App Service created");

            return resource;
        }
        
        private IStorageAccount CreateStorageAccount(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string resourceName)
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
                .Create();
            
            _logger.LogInformation("Storage account created");
            return resource;
        }

        private T CreateFunctionApp(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup,IAppServicePlan appServicePlan , IStorageAccount storageAccount, string resourceName)
        {
            _logger.LogInformation(
                $"Creating function app for the work item with the id {resourceData.Id}");

            var resource = azure.AppServices.FunctionApps
                .Define(resourceName)
                .WithExistingAppServicePlan(appServicePlan)
                .WithExistingResourceGroup(resourceGroup)
                .WithExistingStorageAccount(storageAccount)
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create() as T;

            _logger.LogInformation("Function App created");
            return resource;
        }
        
        public async Task<bool> RemoveResource(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup)
        {
            try
            {
                _logger.LogInformation(
                    $"Deleting resources with the id {resourceData.Id} for the work item with the id {resourceData.Id}");

                var appServices = await azure.AppServices.AppServicePlans
                    .ListByResourceGroupAsync(resourceGroup.Name);
                var appServicePlan = appServices
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());

                var storageAccounts = await azure.StorageAccounts
                    .ListByResourceGroupAsync(resourceGroup.Name);
                var storageAccount = storageAccounts
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());
                
                var functionApps = await azure.AppServices.FunctionApps
                    .ListByResourceGroupAsync(resourceGroup.Name);
                var functionApp = functionApps
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());

                if (functionApp != null) await azure.AppServices.WebApps.DeleteByIdAsync(functionApp.Id);
                if (storageAccount != null) await azure.StorageAccounts.DeleteByIdAsync(storageAccount.Id);
                if (appServicePlan != null) await azure.AppServices.AppServicePlans.DeleteByIdAsync(appServicePlan.Id);
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