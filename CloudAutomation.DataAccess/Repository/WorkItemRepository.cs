using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudAutomation.DataAccess.Contexts;
using CloudAutomation.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudAutomation.DataAccess.Repository
{
    public class WorkItemRepository : Repository<WorkItem>, IWorkItemRepository
    {
        public WorkItemRepository(CloudAutomationContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkItem>> GetAll()
        {
            return await _context.WorkItems.ToListAsync();
        }

        public async Task<WorkItem> GetById(int id)
        {
            return await _context.WorkItems.Where(w => w.Id == id).SingleOrDefaultAsync();
        }
    }
}