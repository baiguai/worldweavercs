using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Input
    {
        public void ParseInput(Classes.Element parentElement, Classes.Element currentElement)
        {
            MainClass.output.MatchMade = false;
            var elemParser = new Elements.Element();
            var elemLogic = new DataManagement.GameLogic.Element();

            Regex rgx = new Regex(currentElement.Syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(MainClass.userInput))
            {
                var procs = ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                foreach (var proc in procs)
                {
                    elemParser.ParseElement(currentElement, proc, false);

                    foreach (var child in currentElement.Children)
                    {
                        var ChildProcs = ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                        foreach (var childProc in ChildProcs)
                        {
                            elemParser.ParseElement(child, childProc);
                            if (MainClass.output.MatchMade)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            return;
        }
    }
}
