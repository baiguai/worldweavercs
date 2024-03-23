using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Parsers.Elements
{
    public class Logic
    {
        public Classes.Output ParseLogic(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            output.MatchMade = true;

            // TODO: Handle the logic

            return output;
        }

        public string ParseSetLogic(string gameDb, string logic, string userInput)
        {
            var output = "";

            output = logic.Replace("[input]", userInput);

            return output;
        }
    }
}