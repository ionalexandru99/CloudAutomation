using System.Threading.Tasks;

namespace CloudAutomation.Application.Interfaces
{
    public interface IHttpClientRequest
    {
        public Task<bool> Execute(string url, string requestUrl);
    }
}