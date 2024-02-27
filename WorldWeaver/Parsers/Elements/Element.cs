using System;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Classes.Output ParseElement(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput, string proc)
        {
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();

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

                        output = input.ParseInput(output, gameDb, currentElement, child, userInput);
                        if (output.MatchMade)
                        {
                            return output;
                        }
                        break;

                    case "enter_message":
                        output = msg.ParseMessage(output, gameDb, child);
                        break;

                    case "message":
                        output = msg.ParseMessage(output, gameDb, child);
                        break;

                    case "enter_action":
                        var action = new Parsers.Elements.Action();
                        output = action.ParseAction(output, gameDb, child, userInput);
                        break;

                    case "action":
                        output = ParseElement(output, gameDb, child, userInput, proc);
                        break;

                    case "move":
                        var move = new Parsers.Elements.Move();
                        output = move.ParseMove(output, gameDb, child, userInput);
                        break;
                }
            }

            return output;
        }
    }
}
