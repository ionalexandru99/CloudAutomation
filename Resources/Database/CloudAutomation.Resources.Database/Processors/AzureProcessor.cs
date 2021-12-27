using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudAutomation.Resources.Database.Data;
using CloudAutomation.Resources.Database.Models;
using CloudAutomation.Resources.Database.Processors.Interfaces;
using CloudAutomation.Resources.Database.Settings;
using CloudAutomation.Utils.Settings;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace CloudAutomation.Resources.Database.Processors
{
    public class AzureProcessor<T> : IAzureProcessor<T> where T : class, ISqlDatabase
    {
        private readonly ServicePrincipalSettings _servicePrincipalSettings;
        private readonly ILogger<AzureProcessor<T>> _logger;
        private readonly IPasswordProcessor _passwordProcessor;
        private readonly KeyVaultSettings _keyVaultSettings;

        private const string ResourceTagWorkItem = "WorkItem";
        private const string ResourceTagWorkItemId = "Id";

        private const string DefaultServerValue = "Select a value";
        private const string AdminAccountSecretName = "{0}-admin-account";

        private const string ConnectionStringTemplate =
            "Server=tcp:{0}.database.windows.net,1433;Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        public AzureProcessor(
            IOptions<ServicePrincipalSettings> servicePrincipalSettings,
            ILogger<AzureProcessor<T>> logger,
            IPasswordProcessor passwordProcessor,
            IOptions<KeyVaultSettings> keyVaultSettings)
        {
            _servicePrincipalSettings = servicePrincipalSettings.Value;
            _logger = logger;
            _passwordProcessor = passwordProcessor;
            _keyVaultSettings = keyVaultSettings.Value;
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
            out T resource,
            out SqlServerCredentials credentials) 
        {
            var generateServer = resourceData.Fields.ServerName.Equals(DefaultServerValue);
            var serverName = generateServer 
                ? SdkContext.RandomResourceName("dbserver", 15)
                : resourceData.Fields.ServerName;
            
            resource = null;
            try
            {
                var server =  generateServer
                    ? CreateServer(azure, resourceData, resourceGroup, serverName) 
                    : GetServer(azure, resourceData, resourceGroup, serverName);
                
                resource = CreateDatabase(
                    azure, 
                    resourceData, 
                    server,
                    out credentials);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error appeared while trying to create the database with the name {resourceData.Fields.Name}");

                RemoveResource(azure, resourceData, resourceGroup, generateServer)
                    .GetAwaiter()
                    .GetResult();

                credentials = default;
                
                return false;
            }

            return true;
        }

        public ISqlServer GetServer(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string serverName)
        {
            _logger.LogInformation(
                $"Getting SQL server with the name {serverName} for the work item with the id {resourceData.Id}");

            var server = azure.SqlServers
                .ListAsync()
                .GetAwaiter()
                .GetResult()
                .SingleOrDefault(x => x.Name == serverName);
            
            _logger.LogInformation("SQL Server retrieved");

            return server;
        }
        
        public ISqlServer GetServer(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup)
        {
            _logger.LogInformation(
                $"Getting SQL server for the work item with the id {resourceData.Id}");

            var server = azure.SqlServers
                .ListByResourceGroup(resourceGroup.Name)
                .SingleOrDefault(x => x.Tags[ResourceTagWorkItemId] == resourceData.Id.ToString());
            
            _logger.LogInformation("SQL Server retrieved");

            return server;
        }

        private ISqlServer CreateServer(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, string serverName)
        {
            _logger.LogInformation(
                $"Creating SQL server with the name {serverName} for the work item with the id {resourceData.Id}");
            
            var username = SdkContext.RandomResourceName(serverName[..5], 10);
            var password = _passwordProcessor
                .GeneratePassword(true, true, true, true, 15);

            var server = azure.SqlServers
                .Define(serverName)
                .WithRegion(Region.Create(resourceGroup.RegionName))
                .WithExistingResourceGroup(resourceGroup)
                .WithAdministratorLogin(username)
                .WithAdministratorPassword(password)
                .WithActiveDirectoryAdministrator(_servicePrincipalSettings.Name, _servicePrincipalSettings.Client)
                .WithNewFirewallRule("0.0.0.0", "255.255.255.255", "All_Internet_Ips")
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create();

            var secretName = GenerateSecretName(serverName, AdminAccountSecretName);
            
            SetSqlServerCredentialInKeyVault(azure, username, password, secretName);
            
            _logger.LogInformation("SQL Server created");

            return server;
        }

        private static string GenerateSecretName(string value, string formattingString)
        {
            var secretName = string.Format(formattingString, value);
            return secretName;
        }

        private void SetSqlServerCredentialInKeyVault(
            IAzure azure, 
            string username, 
            string password, 
            string secretName)
        {
            _logger.LogInformation("Storing credentials in key vault");

            var keyVault = azure.Vaults
                .GetByIdAsync(_keyVaultSettings.Id)
                .GetAwaiter()
                .GetResult();

            var secret = new SqlServerCredentials
            {
                Username = username,
                Password = password
            };

            var secretString = JsonSerializer.Serialize(secret);
            var plainTextBytes = Encoding.UTF8.GetBytes(secretString);
            var base64String = Convert.ToBase64String(plainTextBytes);

            keyVault.Secrets
                .Define(secretName)
                .WithValue(base64String)
                .Create();

            _logger.LogInformation("Credentials stored");
        }
        
        private SqlServerCredentials GetSqlServerCredentialInKeyVault(
            IAzure azure,
            string secretName)
        {
            _logger.LogInformation("Extracting credentials in key vault");

            var keyVault = azure.Vaults
                .GetByIdAsync(_keyVaultSettings.Id)
                .GetAwaiter()
                .GetResult();

            var secret = keyVault.Secrets
                .GetByName(secretName)
                .Value;

            var data = Convert.FromBase64String(secret);
            var decodedString = Encoding.UTF8.GetString(data);
            
            var secretString = JsonSerializer.Deserialize<SqlServerCredentials>(decodedString);
           
            _logger.LogInformation("Credentials stored");

            return secretString;
        }

        private T CreateDatabase(IAzure azure, WorkItem resourceData, ISqlServer server, out SqlServerCredentials credentials)
        {
            _logger.LogInformation(
                $"Creating database with the name {resourceData.Fields.Name} for the work item with the id {resourceData.Id}");

            var edition = resourceData.Fields.Size.Split(" ").First();
            var size = resourceData.Fields.Size.Split(" ").Last();

            var editionSize = edition switch
            {
                "Basic" => (long) (SqlDatabaseBasicStorage) Enum.Parse(typeof(SqlDatabaseBasicStorage), size),
                "Standard" => (long) (SqlDatabaseStandardStorage) Enum.Parse(typeof(SqlDatabaseStandardStorage), size),
                _ => (long) (SqlDatabasePremiumStorage) Enum.Parse(typeof(SqlDatabasePremiumStorage), size)
            };

            var resource = server
                .Databases
                .Define(resourceData.Fields.Name)
                .WithEdition(DatabaseEdition.Parse(edition))
                .WithMaxSizeBytes(editionSize)
                .WithTag(ResourceTagWorkItem, resourceData.Fields.Name)
                .WithTag(ResourceTagWorkItemId, resourceData.Id.ToString())
                .Create() as T;

            var context = ConnectToDatabase(azure, server, resource);
            credentials = CreateDatabaseUser(context);

            _logger.LogInformation("Database created");
            return resource;
        }

        private SqlServerCredentials CreateDatabaseUser(DbContext context)
        {
            _logger.LogInformation("Creating account for database");
            
            try
            {
                var sqlCredentials = new SqlServerCredentials
                {
                    Username = SdkContext.RandomResourceName("user", 10),
                    Password = _passwordProcessor
                        .GeneratePassword(true, true, true, false, 15)
                };

                context.Database.BeginTransaction();

                var result = context
                    .Database
                    .ExecuteSqlRawAsync($"Create user {sqlCredentials.Username} WITH Password='{sqlCredentials.Password}'")
                    .GetAwaiter()
                    .GetResult();

                if (result == 0)
                {
                    throw new Exception("User not created");
                }
                
                result = context
                    .Database
                    .ExecuteSqlRawAsync($"Alter Role db_owner Add member {sqlCredentials.Username}")
                    .GetAwaiter()
                    .GetResult();

                if (result == 0)
                {
                    throw new Exception("User role could not be given");
                }
                
                context.Database.CommitTransaction();
 
                _logger.LogInformation("Database account created");
                
                return sqlCredentials;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not create account in database");
                context.Database.RollbackTransaction();
                throw;
            }
        }

        private DatabaseContext ConnectToDatabase(IAzure azure, ISqlServer server, ISqlDatabase database)
        {
            _logger.LogInformation(
                $"Connecting to database");
            
            var credentials = GetSqlServerCredentialInKeyVault(azure, GenerateSecretName(server.Name, AdminAccountSecretName));
            var connectionString =
                GenerateConnectionString(server.Name, database.Name, credentials.Username, credentials.Password);

            var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(connectionString)
                .Options;

            var context = new DatabaseContext(contextOptions);
            
            _logger.LogInformation(
                $"Connection to database established");

            return context;
        }
        
        public async Task<bool> RemoveResource(IAzure azure, WorkItem resourceData, IResourceGroup resourceGroup, bool generateServer)
        {
            try
            {
                _logger.LogInformation(
                    $"Deleting resources with the id {resourceData.Id} for the work item with the id {resourceData.Id}");
                
                var server = !generateServer 
                    ? GetServer(azure, resourceData, resourceGroup, resourceData.Fields.ServerName) 
                    : GetServer(azure, resourceData, resourceGroup);
                
                if(server != default)
                {
                    var databases = await server
                        .Databases
                        .ListAsync();
                    
                    databases = databases?
                        .Where(x => x.Name == resourceData.Fields.Name)
                        .ToList();
                    
                    
                    var database = databases?.SingleOrDefault();

                    if (database != default) await database.DeleteAsync();

                    if(generateServer)
                    {
                        await azure.SqlServers.DeleteByIdAsync(server.Id);
                        var secretName = GenerateSecretName(server.Name, AdminAccountSecretName);
                        DeleteSqlServerCredentialFromKeyVault(azure, secretName);
                    }
                }
                _logger.LogInformation(
                    $"Deleted resources with the id {resourceData.Id} for the work item with the id {resourceData.Id}");
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

        private static string GenerateConnectionString(string serverName, string databaseName, string userName,
            string password)
        {
            return string.Format(ConnectionStringTemplate, serverName, databaseName, userName, password);
        }
        
        private void DeleteSqlServerCredentialFromKeyVault(
            IAzure azure,
            string secretName)
        {
            _logger.LogInformation("Deleting credentials from key vault");

            var keyVault = azure.Vaults
                .GetByIdAsync(_keyVaultSettings.Id)
                .GetAwaiter()
                .GetResult();

            var secret = keyVault.Secrets
                .GetByName(secretName);
            
            keyVault.Secrets.DeleteById(secret.Id);
            
            _logger.LogInformation("Credentials deleted");
        }
    }
}