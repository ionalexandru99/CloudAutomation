using System.Text.RegularExpressions;
using CloudAutomation.Models.Enums;

namespace CloudAutomation.Utils.Extensions
{
    public static class StateExtensions
    {
        public static string ToExtendedString(this State state)
        {
            var oldValue = state.ToString();
            var newValue = Regex.Replace(oldValue, "([a-z])([A-Z])", "$1 $2");

            return newValue;
        }
    }
}
