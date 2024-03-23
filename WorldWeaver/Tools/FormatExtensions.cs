using System;
using WorldWeaver.Cache;
using WorldWeaver.Parsers;
namespace WorldWeaver.Tools
{
    public static class FormatExtensions
    {
        public static string OutputFormat(this string str)
        {
            if (str == null) return null;
            var output = str;
            var outParser = new Parsers.OutputParser();

            output = output.Replace("---", "--------------------------------------------------------------------------------");
            output = output.Replace("''", "'");

            output = outParser.ParseOutput(Cache.GameCache.GameDb, output);

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
