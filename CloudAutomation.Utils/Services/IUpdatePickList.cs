using System.Threading.Tasks;
using CloudAutomation.Utils.Models;

namespace CloudAutomation.Utils.Services
{
    public interface IUpdatePickList
    {
        Task<bool> Execute(PickList pickList);
    }
}