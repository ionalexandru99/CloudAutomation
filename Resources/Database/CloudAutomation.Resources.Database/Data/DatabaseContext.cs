using Microsoft.EntityFrameworkCore;

namespace CloudAutomation.Resources.Database.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    }
}