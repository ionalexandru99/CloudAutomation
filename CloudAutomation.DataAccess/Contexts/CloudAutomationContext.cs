using CloudAutomation.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudAutomation.DataAccess.Contexts
{
    public class CloudAutomationContext : DbContext
    {
        public CloudAutomationContext(DbContextOptions<CloudAutomationContext> options) : base(options)
        {
        }

        public DbSet<WorkItem> WorkItems { get; set; }
    }
}