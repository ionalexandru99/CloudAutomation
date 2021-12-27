using System;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.Resources.VirtualMachine.Extensions;
using CloudAutomation.Resources.VirtualMachine.Models;
using CloudAutomation.Resources.VirtualMachine.Processors.Interfaces;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.VirtualMachine.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class, IVirtualMachine
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
            string username, 
            string password, 
            out T resource) 
        {
            var virtualMachineName = SdkContext.RandomResourceName("vm-", 15);
            var publicIpDnsLabel = SdkContext.RandomResourceName("pip", 10);
            var nsgName = SdkContext.RandomResourceName("nsg", 10);
            var vnetName = SdkContext.RandomResourceName("vnet", 12);
            var networkInterfaceName = SdkContext.RandomResourceName("nic", 14);
            var publicIpName = SdkContext.RandomResourceName("public", 10);
            
            resource = null;
            try
            {
                var networkSecurityGroup = CreateNetworkSecurityGroup(azure, nsgName, resourceData, resourceGroup);
                var virtualNetwork = CreateVirtualNetwork(azure, vnetName, resourceData, resourceGroup);
                var publicIpAddress = CreatePublicIp(azure, publicIpName, resourceData, resourceGroup);
                var networkInterface = CreateNetworkInterface(
                    azure, 
                    networkInterfaceName, 
                    publicIpDnsLabel, 
                    resourceData, 
                    resourceGroup,
                    virtualNetwork, 
                    networkSecurityGroup,
                    publicIpAddress);
                
                resource = CreateVirtualMachine(
                    azure, 
                    resourceData, 
                    resourceGroup, 
                    username, 
                    password, 
                    virtualMachineName,
                    networkInterface);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the virtual machine with the name {virtualMachineName}");

                RemoveResource(azure, resourceGroup, resourceData)
                    .GetAwaiter()
                    .GetResult();
                
                return false;
            }

            return true;
        }

        private T CreateVirtualMachine(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string username,
            string password, string virtualMachineName, INetworkInterface networkInterface)
        {
            T resource;
            _logger.LogInformation(
                $"Creating virtual machine with the name {virtualMachineName} for the work item with the name {resourceData.Fields.Name}");

            var osDisk = SdkContext.RandomResourceName("os", 10);
            
            var virtualMachine = azure.VirtualMachines
                .Define(virtualMachineName)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithExistingPrimaryNetworkInterface(networkInterface);

            if (IsWindowsImage(resourceData.Fields.VirtualMachineImage))
            {
                resource = virtualMachine
                    .WithPopularWindowsImage(
                        Enum.Parse<KnownWindowsVirtualMachineImage>(
                            resourceData.Fields.VirtualMachineImage))
                    .WithAdminUsername(username)
                    .WithAdminPassword(password)
                    .WithOSDiskName(osDisk)
                    .WithSize(VirtualMachineSizeTypes.Parse(resourceData.Fields.VirtualMachinesSizeType))
                    .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                    .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                    .Create() as T;
            }
            else
            {
                resource = virtualMachine
                    .WithPopularLinuxImage(
                        Enum.Parse<KnownLinuxVirtualMachineImage>(
                            resourceData.Fields.VirtualMachineImage))
                    .WithRootUsername(username)
                    .WithRootPassword(password)
                    .WithOSDiskName(osDisk)
                    .WithSize(VirtualMachineSizeTypes.Parse(resourceData.Fields.VirtualMachinesSizeType))
                    .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                    .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                    .Create() as T;
            }
            
            _logger.LogInformation("Virtual Machine created");
            return resource;
        }
        
        
        public async Task<bool> RemoveResource(IAzure azure, IResourceGroup resourceGroup, WorkItem resourceData)
        {
            try
            {
                _logger.LogInformation(
                    $"Deleting resources with the id {resourceData.Id} for the work item with the name {resourceData.Fields.Name}");

                var virtualMachine = azure.VirtualMachines
                    .ListByResourceGroupAsync(resourceGroup.Name)
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());

                var osDiskId = virtualMachine?.OSDiskId;
                
                var networkInterface = azure.NetworkInterfaces
                    .ListByResourceGroupAsync(resourceGroup.Name)
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());
                
                var publicIp = azure.PublicIPAddresses
                    .ListByResourceGroupAsync(resourceGroup.Name)
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());
                
                var virtualNetwork = azure.Networks
                    .ListByResourceGroupAsync(resourceGroup.Name)
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());
                
                var networkSecurityGroup = azure.NetworkSecurityGroups
                    .ListByResourceGroupAsync(resourceGroup.Name)
                    .GetAwaiter()
                    .GetResult()
                    .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());

                if (virtualMachine != null) await azure.VirtualMachines.DeleteByIdAsync(virtualMachine.Id);
                if (osDiskId != null) await azure.Disks.DeleteByIdAsync(osDiskId);
                if (networkInterface != null) await azure.NetworkInterfaces.DeleteByIdAsync(networkInterface.Id);
                if (publicIp != null) await azure.PublicIPAddresses.DeleteByIdAsync(publicIp.Id);
                if (virtualNetwork != null) await azure.Networks.DeleteByIdAsync(virtualNetwork.Id);
                if (networkSecurityGroup != null)
                    await azure.NetworkSecurityGroups.DeleteByIdAsync(networkSecurityGroup.Id);
                
                _logger.LogInformation(
                    $"Deleted resources for the work item with the name {resourceData.Fields.Name}");
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
        
        private static bool IsWindowsImage(string virtualMachineImage) =>
            Enum.TryParse<KnownWindowsVirtualMachineImage>(
                virtualMachineImage, out _);
        
        private INetworkSecurityGroup CreateNetworkSecurityGroup(IAzure azure, string nsgName, WorkItem resourceData, IResourceGroup resourceGroup)
        {
            _logger.LogInformation($"Creating network security group with the name {nsgName} for the work item with the name {resourceData.Fields.Name}");

            var networkSecurityGroup = azure.NetworkSecurityGroups
                .Define(nsgName)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .DefineRule("Allow_SSH", 22, 1000)
                .DefineRule("Allow_Https", 443 , 1001)
                .DefineRule("Allow_Rdp", 3389, 1002)
                .Create();
            
            _logger.LogInformation("Network security group created");

            return networkSecurityGroup;
        }

        private INetwork CreateVirtualNetwork(IAzure azure, string name, WorkItem resourceData, IResourceGroup resourceGroup)
        {
            _logger.LogInformation($"Creating virtual network with the name {name} for the work item with the name {resourceData.Fields.Name}");
            
            var vnet = azure.Networks
                .Define(name)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithAddressSpace("10.0.0.0/28")
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create();
            
            _logger.LogInformation("Virtual network created");

            return vnet;
        }
        
        private INetworkInterface CreateNetworkInterface(
            IAzure azure, 
            string name,
            string dnsLabel,
            WorkItem resourceData, 
            IResourceGroup resourceGroup, 
            INetwork vnet, 
            INetworkSecurityGroup networkSecurityGroup,
            IPublicIPAddress publicIpAddress)
        {
            _logger.LogInformation($"Creating network interface with the name {name} for the work item with the name {resourceData.Fields.Name}");

            var networkInterface = azure.NetworkInterfaces
                .Define(name)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithExistingPrimaryNetwork(vnet)
                .WithSubnet(vnet.Subnets.First().Value.Name)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingNetworkSecurityGroup(networkSecurityGroup)
                .WithInternalDnsNameLabel(dnsLabel)
                .WithExistingPrimaryPublicIPAddress(publicIpAddress)
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create();
            
            _logger.LogInformation("Network interface created");

            return networkInterface;
        }

        private IPublicIPAddress CreatePublicIp(IAzure azure,
            string name,
            WorkItem resourceData,
            IResourceGroup resourceGroup)
        {
            _logger.LogInformation($"Creating public ip with the name {name} for the work item with the name {resourceData.Fields.Name}");

            var publicIp = azure.PublicIPAddresses
                .Define(name)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithStaticIP()
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create();
            
            _logger.LogInformation("Public interface created");

            return publicIp;
        }
    }
}