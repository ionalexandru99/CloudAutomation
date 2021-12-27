using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudAutomation.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        public Task AddAsync(T element);
        public Task Remove(T element);
        public Task RemoveRange(IEnumerable<T> element);
        public void CommitAsync();
        public Task AddRangeAsync(IEnumerable<T> elements);
        public Task Update(T element);
        public Task UpdateRange(IEnumerable<T> elements);
    }
}