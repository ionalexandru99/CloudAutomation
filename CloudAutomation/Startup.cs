using System;
using CloudAutomation.Application.Factories;
using CloudAutomation.Application.Implementations;
using CloudAutomation.Application.Implementations.Encryption;
using CloudAutomation.Application.Implementations.File;
using CloudAutomation.Application.Implementations.Smtp;
using CloudAutomation.Application.Interfaces;
using CloudAutomation.Application.Interfaces.Encryption;
using CloudAutomation.Application.Interfaces.File;
using CloudAutomation.Application.Interfaces.Smtp;
using CloudAutomation.Application.Processors;
using CloudAutomation.Application.Services;
using CloudAutomation.DataAccess.Contexts;
using CloudAutomation.DataAccess.DependencyInjection;
using CloudAutomation.Models.DevOps;
using CloudAutomation.Utils.Configurations;
using CloudAutomation.Utils.Enums;
using CloudAutomation.Utils.Services;
using CloudAutomation.Utils.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CloudAutomation
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
            services.AddRazorPages();

            services.AddHostedService<ServiceBusConsumer>();
            
            services.AddSingleton<ISubscriptionClient>
            (
                x =>
                    new SubscriptionClient
                    (
                        Configuration.GetValue<string>("ServiceBusRetriever:ConnectionString"),
                        Configuration.GetValue<string>("ServiceBusRetriever:TopicName"),
                        Configuration.GetValue<string>("ServiceBusRetriever:SubscriptionName")
                    )
                    {
                        OperationTimeout = new TimeSpan(0,0,30,0)
                    }
            );

            services.AddSingleton<IWorkItemProcessor, WorkItemProcessor>();
            services.AddSingleton<IHttpClientRequest, WorkItemClient>();
            services.AddSingleton<ICustomServiceProcessor, CustomServiceProcessor>();

            services.AddTransient<ISmtpService, SmtpService>();
            services.AddTransient<IEmailFactory, EmailFactory>();
            services.AddTransient<IEncryption, Encryption>();
            services.AddTransient<IReadFile, ReadFile>();
            services.AddTransient<IGetWorkItem<Resource>, GetWorkItem<Resource>>();
            services.AddTransient<IServiceBusSender, ServiceBusSender>();
            services.AddTransient<IUpdateStateClient, UpdateStateClient>();
            services.AddTransient<IServiceBusConfiguration, ServiceBusConfiguration>();

            services.Configure<EmailAccount>(Configuration.GetSection("EmailAccount"));
            services.Configure<DevOpsSettings>(Configuration.GetSection("DevOps"));
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<EncryptionSettings>(Configuration.GetSection("EncryptionSettings"));
            services.Configure<ServiceBusAccounts>(Configuration.GetSection("ServiceBusAccount"));
            services.Configure<CloudTeamSettings>(Configuration.GetSection("CloudTeamSettings"));


            services.AddDbContext<CloudAutomationContext>
            (options =>
                    options.UseSqlServer
                    (
                        Configuration["DataBase:ConnectionString"],
                        x => x.MigrationsAssembly("CloudAutomation.DataAccess")
                    ),
                ServiceLifetime.Singleton
            );

            services.AddRepositories();
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