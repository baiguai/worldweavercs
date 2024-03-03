using System;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Action
    {
        internal Classes.Output ParseAction(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var elem = new Parsers.Elements.Element();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);

            foreach (var child in currentElement.Children)
            {
                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, currentElement, userInput, proc);
                }
                if (output.MatchMade)
                {
                    break;
                }
            }

            return output;
        }
    }
}
