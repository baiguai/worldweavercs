using System;
namespace WorldWeaver.Tools
{
    public static class FormatExtensions
    {
        public static string OutputFormat(this string str)
        {
            if (str == null) return null;
            var output = str;

            output = output.Replace("---", "--------------------------------------------------------------------------------");
            output = output.Replace("''", "'");

            return output;
        }

        public static string FormatDate(this DateTime dt)
        {
            if (dt == null) return null;
            var output = "";

            output = dt.ToString("yyyyMMdd");

            return output;
        }
    }
}
