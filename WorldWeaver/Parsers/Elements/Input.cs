﻿using System;
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
            var syntax = Tools.OutputProcessor.ProcessSpecialValue(currentElement, currentElement.Syntax);
            if (syntax.Equals(""))
            {
                return;
            }

            Regex rgx = new Regex(syntax, RegexOptions.IgnoreCase);

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

                    if (MainClass.gameDb.Equals(""))
                    {
                        return;
                    }

                    elemParser.ParseElement(currentElement, proc, false);
                }
            }

            return;
        }
    }
}
