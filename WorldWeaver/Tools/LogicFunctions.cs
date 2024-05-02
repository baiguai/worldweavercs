using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Tools
{
    public class LogicFunctions
    {
        public static List<string> GetParentAndField(string logicString, string openString, string closeString)
        {
            var outputList = new List<string>();

            var logArr = logicString.Split(openString);
            if (logArr.Length == 2)
            {
                outputList.Add(logArr[0].Trim());
                outputList.Add(logArr[1].Replace(closeString, "").Trim());
                return outputList;
            }
            else
            {
                outputList.Add(logicString);
                return outputList;
            }
        }
    }
}