using System.Threading.Tasks;
using CloudAutomation.Models.Enums;

namespace CloudAutomation.Utils.Services
{
    public interface IUpdateStateClient
    {
        Task<bool> Execute(string resourceId, State state);
    }
}
