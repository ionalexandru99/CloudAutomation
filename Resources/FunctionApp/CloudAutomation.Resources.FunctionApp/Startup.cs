using CloudAutomation.Resources.FunctionApp.Clients;
using CloudAutomation.Resources.FunctionApp.Factories;
using CloudAutomation.Resources.FunctionApp.Models;
using CloudAutomation.Resources.FunctionApp.Processors;
using CloudAutomation.Resources.FunctionApp.Processors.Interfaces;
using CloudAutomation.Resources.FunctionApp.Services;
using CloudAutomation.Utils.Configurations;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ApprovalSettings = CloudAutomation.Utils.Settings.ApprovalSettings;

namespace CloudAutomation.Resources.FunctionApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHostedService<ApprovalServiceBusConsumer>();
            services.AddHostedService<CreationServiceBusConsumer>();
            services.AddHostedService<RejectedServiceBusConsumer>();

            services.AddSingleton<IResourceProcessor<IFunctionApp>, ResourceProcessor<IFunctionApp>>();
            services.AddSingleton<IUpdateStateClient, UpdateStateClient>();
            services.AddSingleton<IValidationProcessor, ValidationProcessor>();
            services.AddSingleton<IGetPickList, GetPickList>();
            services.AddSingleton<IUpdatePickList, UpdatePickList>();
            services.AddSingleton<IFinalizeProcessor, FinalizeProcessor>();
            services.AddSingleton<IAzureProcessor<IFunctionApp>, AzureProcessor<IFunctionApp>>();
            services.AddSingleton<IRemoveProcessor, RemoveProcessor>();

            services.AddTransient<IGetWorkItem<WorkItem>, GetWorkItem<WorkItem>>();
            services.AddTransient<IEmailFactory, EmailFactory>();
            services.AddTransient<IEmailClient, EmailClient>();
            services.AddTransient<IServiceBusConfiguration, ServiceBusConfiguration>();

            services.Configure<ApprovalSettings>(Configuration.GetSection("ApprovalSettings"));
            services.Configure<DevOpsSettings>(Configuration.GetSection("DevOps"));
            services.Configure<ServicePrincipalSettings>(Configuration.GetSection("ServicePrincipal"));
            services.Configure<ServiceBusAccounts>(Configuration.GetSection("ServiceBusAccount"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}