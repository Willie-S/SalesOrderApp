using System.Globalization;

namespace SalesOrderApp.Utilities
{
    public static class GeneralHelper
    {
        public static bool CompareStrings(string value1, string value2)
        {
            return value1?.Trim().ToLower() == value2?.Trim().ToLower();
        }

        public static string NormaliseString(string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value?.Trim().ToLower());
        }

        public static string NormaliseStringForEmail(string value)
        {
            return value?.Trim().ToLower();
        }
    }
}
