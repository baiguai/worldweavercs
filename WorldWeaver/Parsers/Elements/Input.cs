using System;
using System.Text.RegularExpressions;

namespace WorldWeaver.Parsers.Elements
{
    public class Input
    {
        public Classes.Output ParseInput(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            output.MatchMade = false;

            Regex rgx = new Regex(currentElement.Syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(userInput))
            {
                var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                foreach (var proc in procItems)
                {
                    foreach (var child in currentElement.Children)
                    {}
                }
            }

            return output;
        }
    }
}
