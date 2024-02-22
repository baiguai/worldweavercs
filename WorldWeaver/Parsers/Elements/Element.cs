using System;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Classes.Output ParseElement(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.element_type);

            foreach (var proc in procItems)
            { 
                foreach (var child in currentElement.children)
                {
                    if (output.MatchMade)
                    {
                        return output;
                    }

                    switch (child.element_type)
                    {
                        case "input":
                            var input = new Parsers.Elements.Input();
                            output = input.ParseInput(gameDb, currentElement, child, userInput);
                            if (output.MatchMade)
                            {
                                return output;
                            }
                            break;
                    }

                    output = ParseElement(output, gameDb, child, userInput);
                }
            }

            return output;
        }
    }
}
