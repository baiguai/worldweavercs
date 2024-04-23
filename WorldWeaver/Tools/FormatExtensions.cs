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

        public static string Randomize(this string value)
        {
            var output = "";

            if (value.Contains("[rand:"))
            {
                var tmp = value.Replace("[rand:", "").Replace("]", "");
                var range = tmp.Split('|');
                if (range.Length == 2)
                {
                    Random rnd = new Random((int)DateTime.Now.Ticks);
                    output = rnd.Next(Convert.ToInt32(range[0]), Convert.ToInt32(range[1])).ToString();
                }
            }

            return output;
        }

        public static int RandomValue(this string value)
        {
            var rndVal = 0;

            if (value.Contains("[rand:"))
            {
                var tmp = value.Replace("[rand:", "").Replace("]", "");
                var range = tmp.Split('|');
                if (range.Length == 2)
                {
                    Random rnd = new Random((int)DateTime.Now.Ticks);
                    rndVal = rnd.Next(Convert.ToInt32(range[0]), Convert.ToInt32(range[1]));
                }
            }

            return rndVal;
        }
    }
}
