using System.Collections.Generic;
using System.Threading.Tasks;
using CloudAutomation.DataAccess.Contexts;

namespace CloudAutomation.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CloudAutomationContext _context;

        protected Repository(CloudAutomationContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T element)
        {
            await _context.AddAsync(element);
        }

        public Task Remove(T element)
        {
            _context.Remove(element);
            return Task.CompletedTask;
        }

        public Task RemoveRange(IEnumerable<T> elements)
        {
            _context.RemoveRange(elements);
            return Task.CompletedTask;
        }

        public async void CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> elements)
        {
            await _context.AddRangeAsync(elements);
        }

        public Task Update(T element)
        {
            _context.Update(element);
            return Task.CompletedTask;
        }

        public Task UpdateRange(IEnumerable<T> elements)
        {
            _context.UpdateRange(elements);
            return Task.CompletedTask;
        }
    }
}