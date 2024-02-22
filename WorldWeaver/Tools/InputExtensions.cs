using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldWeaver.Tools
{
    public static class InputExtensions
    {
        public static string GetInputParamSingle(this string input)
        {
            List<string> parms = input.Split(' ').ToList();
            parms.RemoveAt(0);
            var output = string.Join<string>("_", parms);
            return output;
        }

        public static string GetInputParams(this string input)
        {
            List<string> parms = input.Split(' ').ToList();
            parms.RemoveAt(0);
            var output = string.Join<string>(" ", parms);
            return output;
        }

        public static List<string> GetInputParamsAsList(this string input)
        {
            List<string> parms = input.Split(' ').ToList();
            return parms;
        }
    }
}
