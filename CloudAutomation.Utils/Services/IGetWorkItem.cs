using System.Threading.Tasks;

namespace CloudAutomation.Utils.Services
{
    public interface IGetWorkItem<T> where T : class
    {
        public Task<T> GetWorkItemByUri(string uri, string pat);
        public Task<T> GetWorkItemById(string resourceId, string pat);
    }
}