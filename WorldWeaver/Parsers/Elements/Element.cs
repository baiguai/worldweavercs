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
                    if (!child.element_type.Equals(proc))
                    {
                        continue;
                    }

                    switch (child.element_type)
                    {
                        case "input":
                            if (output.MatchMade)
                            {
                                return output;
                            }

                            var input = new Parsers.Elements.Input();
                            output = input.ParseInput(output, gameDb, currentElement, child, userInput);
                            if (output.MatchMade)
                            {
                                return output;
                            }
                            break;

                        case "message":
                        case "enter_message":
                            var msg = new Parsers.Elements.Message();
                            output = msg.ParseMessage(output, gameDb, child);
                            break;

                        case "move":
                            var move = new Parsers.Elements.Move();
                            output = move.ParseMove(output, gameDb, child, userInput);
                            break;
                    }

                    output = ParseElement(output, gameDb, child, userInput);
                }
            }

            return output;
        }
    }
}
