using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.WebApplication.Models;
using CloudAutomation.Resources.WebApplication.Processors.Interfaces;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OperatingSystem = Microsoft.Azure.Management.AppService.Fluent.OperatingSystem;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.WebApplication.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class, IWebApp
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
            var resourceName = SdkContext.RandomResourceName("cloudweb", 15);
            var appServiceName = SdkContext.RandomResourceName("appservice", 15);
            resource = null;
            try
            {
                var appService = CreateAppService(azure, resourceData, resourceGroup, appServiceName);
                
                resource = CreateWebApplication(
                    azure, 
                    resourceData, 
                    resourceGroup, 
                    appService, 
                    resourceName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the web application for the work item with the id {resourceData.Id}");

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

        private T CreateWebApplication(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup,IAppServicePlan appServicePlan ,string resourceName)
        {
            _logger.LogInformation(
                $"Creating web application for the work item with the id {resourceData.Id}");

            var resource = azure.AppServices.WebApps
                .Define(resourceName)
                .WithExistingWindowsPlan(appServicePlan)
                .WithExistingResourceGroup(resourceGroup)
                .WithRuntimeStack(
                    (WebAppRuntimeStack) typeof(WebAppRuntimeStack).GetField(resourceData.Fields.RuntimeStack)?.GetValue(null))
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create() as T;
       
            _logger.LogInformation("Web Application created");
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

                var webApps = await azure.AppServices.WebApps
                    .ListByResourceGroupAsync(resourceGroup.Name);
                var webApp = webApps
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());

                if (webApp != null) await azure.AppServices.WebApps.DeleteByIdAsync(webApp.Id);
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