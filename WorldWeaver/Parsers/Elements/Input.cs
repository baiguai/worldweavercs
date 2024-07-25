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
            var elemLogic = new Parsers.Elements.Logic();

            Regex rgx = new Regex(currentElement.Syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(MainClass.userInput))
            {
                var procs = ProcFunctions.GetProcessStepsByType(currentElement.ElementType);
                foreach (var proc in procs)
                {
                    if (proc.ChildProcElements.Contains("logic"))
                    {
                        foreach (var child in currentElement.Children.Where(c => c.ElementType.Equals("logic")))
                        {
                            elemLogic.ParseLogic(child);
                            if (MainClass.output.FailedLogic)
                            {
                                MainClass.output.MatchMade = true;
                                return;
                            }
                        }
                    }
                    elemParser.ParseElement(currentElement, proc, false);
                }
            }

            return;
        }
    }
}
