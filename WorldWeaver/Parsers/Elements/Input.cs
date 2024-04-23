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
                var procs = ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                foreach (var proc in procs)
                {
                    output = elemParser.ParseElement(output, gameDb, currentElement, userInput, proc, false);

                    foreach (var child in currentElement.Children)
                    {
                        var ChildProcs = ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                        foreach (var childProc in ChildProcs)
                        {
                            output = elemParser.ParseElement(output, gameDb, child, userInput, childProc);
                            if (output.MatchMade)
                            {
                                return output;
                            }
                        }
                    }
                }
            }

            return output;
        }
    }
}
