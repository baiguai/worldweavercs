using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Input
    {
        public Classes.Output ParseInput(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            output.MatchMade = false;
            var elemParser = new Elements.Element();
            var elemLogic = new DataManagement.GameLogic.Element();

            Regex rgx = new Regex(currentElement.Syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(userInput))
            {
                var elemChildren = currentElement.Children;

                foreach (var child in elemChildren)
                {
                    var procs = ProcFunctions.GetProcessStepsByType(child.ElementType);
                    foreach (var proc in procs)
                    {
                        output = elemParser.ParseElement(output, gameDb, child, userInput, proc, false);
                    }
                }
            }

            return output;
        }
    }
}
