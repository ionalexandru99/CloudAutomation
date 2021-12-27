using System.Threading.Tasks;
using CloudAutomation.Utils.Models;

namespace CloudAutomation.Utils.Services
{
    public interface IGetPickList
    {
        Task<PickList> Execute(string id);
    }
}