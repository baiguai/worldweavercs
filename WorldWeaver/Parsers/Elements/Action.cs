using System;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Action
    {
        internal Classes.Output ParseAction(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var elem = new Parsers.Elements.Element();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.element_type);

            foreach (var child in currentElement.children)
            {
                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, child, userInput, proc);
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
