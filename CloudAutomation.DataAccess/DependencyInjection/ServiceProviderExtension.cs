using CloudAutomation.DataAccess.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace CloudAutomation.DataAccess.DependencyInjection
{
    public static class ServiceProviderExtension
    {
        public static void AddRepositories(this IServiceCollection service)
        {
            service.AddSingleton<IWorkItemRepository, WorkItemRepository>();
        }
    }
}