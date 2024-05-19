using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Tools
{
    public class RegexTools
    {
        public static string RegexScrub(string regex)
        {
            regex = regex.Replace("\\b", "");

            return regex;
        }
    }
}