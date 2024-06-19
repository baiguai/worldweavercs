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
            var formattedOutput = str;
            var outParser = new Parsers.OutputParser();

            formattedOutput = formattedOutput.Replace("---", "--------------------------------------------------------------------------------");
            formattedOutput = formattedOutput.Replace("''", "'");
            formattedOutput = formattedOutput.Replace("\\b", $"{Environment.NewLine}");

            return formattedOutput;
        }

        public static string FormatDate(this DateTime dt)
        {
            if (dt == null) return null;
            var output = "";

            output = dt.ToString("yyyyMMdd");

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
            else
            {
                rndVal = RollDice(value);
            }

            return rndVal;
        }

        public static int RollDice(this string value)
        {
            var rndVal = -1;

            if (value.Contains("[roll:"))
            {
                var tmp = value.Replace("[roll:", "").Replace("]", "").ToLower();
                var diceSpec = tmp.Split('d');
                try
                {
                    if (diceSpec.Length == 2)
                    {
                        Random rnd = new Random((int)DateTime.Now.Ticks);
                        var numOfDice = Convert.ToInt32(diceSpec[0].Trim());
                        var sides = Convert.ToInt32(diceSpec[1].Trim());
                        var total = 0;

                        for (int rolls = 0; rolls <= numOfDice; rolls++)
                        {
                            var rollVal = rnd.Next(1, sides + 1);
                            total = total += rollVal;
                        }

                        rndVal = total;
                    }
                }
                catch (Exception)
                {
                    return rndVal;
                }
            }

            return rndVal;
        }
    }
}
