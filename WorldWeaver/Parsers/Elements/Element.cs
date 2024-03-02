using System;
using System.Collections.Generic;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Classes.Output ParseElement(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput, string proc)
        {
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();
            var rptType = currentElement.repeat;
            var index = -1;
            var handledMessage = false;
            var handledMove = false;

            for (var ix = 0; ix < currentElement.children.Count; ix++)
            {
                var child = currentElement.children[ix];

                if (!child.element_type.Equals(proc))
                {
                    continue;
                }

                index = HandleRepeat(gameDb, currentElement, currentElement.children, rptType);

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
                        if (!handledMessage && ix == index)
                        {
                            output = msg.ParseMessage(output, gameDb, child);
                            handledMessage = true;
                        }
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
                        if (!handledMove && ix == index)
                        {
                            var move = new Parsers.Elements.Move();
                            output = move.ParseMove(output, gameDb, child, userInput);
                            handledMove = true;
                        }
                        break;
                }
            }

            return output;
        }

        private int HandleRepeat(string gameDb, Classes.Element currentElement, List<Classes.Element> children, string rptType)
        {
            var output = 0;
            DataManagement.GameLogic.Element dbElem = new DataManagement.GameLogic.Element();
            var index = currentElement.repeat_index;

            switch (rptType.Trim().ToLower())
            {
                case "none":
                    if (index == children.Count)
                    {
                        output = children.Count - 1;
                    }
                    else
                    {
                        index++;
                        output = index;
                    }
                    break;

                case "random":
                    if (index == -1)
                    {
                        var rnd = new Random((int)DateTime.Now.Ticks);
                        output = rnd.Next(0, children.Count - 1);
                    }
                    else
                    {
                        output = index;
                    }
                    break;

                case "repeat":
                    if (index >= children.Count - 1)
                    {
                        output = 0;
                    }
                    else
                    {
                        index++;
                        output = index;
                    }
                    break;

                default:
                    output = 0;
                    break;
            }

            currentElement.repeat_index = output;
            dbElem.SetElementField(gameDb, currentElement.element_key, "repeat_index", output.ToString());
            return output;
        }
    }
}
