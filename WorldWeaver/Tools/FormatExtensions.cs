using System;
using WorldWeaver.Cache;
using WorldWeaver.Classes;
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

        public static string RandomSplitValue(this string value)
        {
            var arr = value.Split("||");
            Random rnd = new Random((int)DateTime.Now.Ticks);
            var rndIdx = rnd.Next(0, arr.Length-1);
            var rndVal = arr[rndIdx];

            return rndVal;
        }

        public static int RandomValue(this string value, Classes.Element currentElement)
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
                rndVal = RollDice(value, currentElement);
            }

            return rndVal;
        }

        public static int RollDice(this string value, Classes.Element currentElement)
        {
            var rndVal = -1;
            var rollVals = new List<int>();
            var totalNumOfDice = 0;
            var modValue = 0;

            if (value.Contains("[roll:"))
            {
                var tmp = value.Replace("[roll:", "").Replace("]", "").ToLower();

                if (tmp.Contains("+") || tmp.Contains("-"))
                {
                    var delimiter = "+";
                    if (tmp.Contains("-"))
                    {
                        delimiter = "-";
                    }
                    modValue = ProcessModifier(currentElement, delimiter, tmp);
                    var arr = tmp.Split(delimiter);
                    if (arr.Length == 2)
                    {
                        tmp = arr[0].Trim();
                    }
                }

                if (value.Contains("(") && value.Contains(")"))
                {
                    var arr = value.Split(')');
                    if (arr.Length == 2)
                    {
                        value = arr[1].Trim();
                        arr[0] = arr[0].Replace("(", "");
                        try
                        {
                            totalNumOfDice = Convert.ToInt32(arr[0]);
                        }
                        catch (Exception)
                        {
                            totalNumOfDice = 0;
                        }
                    }
                }

                var diceSpec = tmp.Split('d');
                try
                {
                    if (diceSpec.Length == 2)
                    {
                        Random rnd = new Random((int)DateTime.Now.Ticks);
                        var numOfDice = Convert.ToInt32(diceSpec[0].Trim());
                        var sides = Convert.ToInt32(diceSpec[1].Trim());
                        var total = 0;
                        var rollNum = numOfDice;
                        if (totalNumOfDice > numOfDice)
                        {
                            rollNum = totalNumOfDice;
                        }

                        for (int rolls = 0; rolls <= rollNum; rolls++)
                        {
                            var rollVal = rnd.Next(1, sides + 1);
                            rollVals.Add(rollVal);
                        }

                        if (totalNumOfDice > numOfDice)
                        {
                            var taken = (from i in rollVals orderby i descending select i).Take(numOfDice);
                            total = taken.Sum();
                        }
                        else {
                            total = rollVals.Sum();
                        }

                        rndVal = total;

                        total = total + modValue;
                    }
                }
                catch (Exception)
                {
                    return rndVal;
                }
            }

            return rndVal;
        }

        private static int ProcessModifier(Element currentElement, string delimiter, string value)
        {
            var arr = value.Split(delimiter);
            var modifier = 0;

            if (arr.Length != 2)
            {
                return 0;
            }

            try{
                modifier = Convert.ToInt32(Tools.OutputProcessor.ProcessSpecialValues(arr[1].Trim(), currentElement));
                if (delimiter.Equals("-"))
                {
                    modifier = -(modifier);
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return modifier;
        }
    }
}
