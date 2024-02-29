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

                    case "message":
                        output = msg.ParseMessage(output, gameDb, child);
                        break;

                    case "action":
                        var action = new Parsers.Elements.Action();
                        if (child.logic.Equals(""))
                        {
                            output = action.ParseAction(output, gameDb, child, userInput);
                        }
                        else
                        {
                            var el = new DataManagement.GameLogic.Element();
                            var target = el.GetElementByKey(gameDb, child.logic);
                            var procItems = Tools.ProcFunctions.GetProcessStepsByType(target.element_type);

                            foreach (var actionProc in procItems)
                            {
                                output = ParseElement(output, gameDb, target, userInput, actionProc);
                            }
                        }
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
