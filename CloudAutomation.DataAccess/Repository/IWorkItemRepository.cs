using System.Collections.Generic;
using System.Threading.Tasks;
using CloudAutomation.DataAccess.Models;

namespace CloudAutomation.DataAccess.Repository
{
    public interface IWorkItemRepository : IRepository<WorkItem>
    {
        public Task<IEnumerable<WorkItem>> GetAll();
        public Task<WorkItem> GetById(int id);
    }
}