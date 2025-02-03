using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Tools
{
    public class ValueTools
    {
        public static int Randomize(int minValue, int maxValue)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            return rnd.Next(minValue, maxValue);
        }

        public static string JoinList(List<string> list)
        {
            string listOutput = "";

            foreach (string itm in list)
            {
                if (!listOutput.Equals(""))
                {
                    listOutput += Environment.NewLine;
                }
                else
                {
                    listOutput += itm;
                }
            }

            return listOutput;
        }
    }
}