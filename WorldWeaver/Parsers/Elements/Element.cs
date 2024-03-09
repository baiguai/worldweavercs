using System;
using System.Collections.Generic;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Classes.Output ParseElement(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput, Classes.ElementProc procObj)
        {
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();
            var rptType = currentElement.Repeat;
            var index = -1;
            var handledMessage = false;
            var handledMove = false;

            foreach (var proc in procObj.ChildProcElements)
            {
                foreach (var child in currentElement.Children)
                {
                    var currentIndex = 0;
                    var currentType = "";
                    handledMessage = false;
                    handledMove = false;

                    if (!child.ElementType.Equals(proc))
                    {
                        continue;
                    }

                    index = HandleRepeat(gameDb, child, child.Children, rptType);

                    if (child.ElementType.Equals(currentType))
                    {
                        currentIndex++;
                    }
                    else
                    {
                        currentType = child.ElementType;
                        currentIndex = 0;
                    }

                    switch (child.ElementType)
                    {
                        case "input":
                            if (output.MatchMade)
                            {
                                return output;
                            }

                            if (!procObj.AllowRepeatOptions || currentIndex == index)
                            {
                                output = input.ParseInput(output, gameDb, currentElement, child, userInput);
                                if (output.MatchMade)
                                {
                                    return output;
                                }
                            }
                            break;

                        case "message":
                        case "enter_message":
                            if (!handledMessage && (!procObj.AllowRepeatOptions || currentIndex == index))
                            {
                                output = msg.ParseMessage(output, gameDb, child);
                                if (!child.Output.Equals(""))
                                {
                                    handledMessage = true;
                                }
                            }
                            break;

                        case "action":
                            if (!procObj.AllowRepeatOptions || currentIndex == index)
                            {
                                output = HandleAction(output, gameDb, child, userInput);
                            }
                            break;

                        case "logic":
                            var lgc = new Parsers.Elements.Logic();

                            output = lgc.ParseLogic(output, gameDb, child, userInput);

                            break;

                        case "move":
                            if (!procObj.AllowRepeatOptions || currentIndex == index)
                            {
                                if (!handledMove && currentIndex == index)
                                {
                                    var move = new Parsers.Elements.Move();
                                    output = move.ParseMove(output, gameDb, child, userInput);
                                    handledMove = true;
                                }
                            }
                            break;
                    }
                }
            }

            return output;
        }

        private Output HandleAction(Output output, string gameDb, Classes.Element child, string userInput)
        {
            var action = new Parsers.Elements.Action();
            if (child.Logic.Equals(""))
            {
                var procItems = Tools.ProcFunctions.GetProcessStepsByType(child.ElementType);

                foreach (var actionProc in procItems)
                {
                    output = ParseElement(output, gameDb, child, userInput, actionProc);
                }
                return output;
            }
            else
            {
                var el = new DataManagement.GameLogic.Element();
                var target = el.GetElementByKey(gameDb, child.Logic);
                var procItems = Tools.ProcFunctions.GetProcessStepsByType(target.ElementType);

                foreach (var actionProc in procItems)
                {
                    output = ParseElement(output, gameDb, target, userInput, actionProc);
                }
            }

            return output;
        }

        private int HandleRepeat(string gameDb, Classes.Element currentElement, List<Classes.Element> children, string rptType)
        {
            var output = 0;
            DataManagement.GameLogic.Element dbElem = new DataManagement.GameLogic.Element();
            var index = currentElement.RepeatIndex;

            if (children.Count < 2)
            {
                return 0;
            }

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

            currentElement.RepeatIndex = output;
            dbElem.SetElementField(gameDb, currentElement.ElementKey, "RepeatIndex", output.ToString());
            return output;
        }
    }
}
