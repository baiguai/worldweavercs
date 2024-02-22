using System;
namespace WorldWeaver.Tools
{
    public static class DbExtensions
    {
        public static string SqlSafe(this string str)
        {
            if (str == null) return null;
            var output = str;

            output = output.Replace("'", "''");

            return output;
        }
    }
}
