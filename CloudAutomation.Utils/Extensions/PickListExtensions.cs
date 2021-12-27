using CloudAutomation.Utils.Models;

namespace CloudAutomation.Utils.Extensions
{
    public static class PickListExtensions
    {
        public static void AddItem(this PickList pickList, string item)
        {
            pickList.Items.Add(item);
        }
    }
}