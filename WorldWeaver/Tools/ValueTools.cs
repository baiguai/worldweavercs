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
    }
}