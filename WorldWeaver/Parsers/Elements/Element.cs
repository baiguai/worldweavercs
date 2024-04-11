using System;
using System.Collections.Generic;
using System.Data.Common;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Classes.Output ParseElement(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput, Classes.ElementProc procObj, bool isEntering = false)
        {
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();
            var index = -1;
            var handledMessage = false;
            var handledMove = false;

            foreach (var proc in procObj.ChildProcElements)
            {
                handledMessage = false;

                foreach (var child in currentElement.Children)
                {
                    handledMove = false;

                    if (!child.ElementType.Equals(proc))
                    {
                        continue;
                    }

                    switch (child.ElementType)
                    {
                        case "input":
                            if (output.MatchMade && !output.OutputText.Equals(""))
                            {
                                continue;
                            }

                            output = input.ParseInput(output, gameDb, currentElement, child, userInput);
                            if (output.MatchMade)
                            {
                                return output;
                            }
                            break;

                        case "message":
                        case "enter_message":
                            if (child.ElementType.Equals("enter_message") && !isEntering)
                            {
                                continue;
                            }
                            if (handledMessage)
                            {
                                continue;
                            }

                            output.MatchMade = false;
                            output = HandleMessage(output, gameDb, currentElement, child, userInput, procObj.AllowRepeatOptions, isEntering);
                            if (output.MatchMade)
                            {
                                handledMessage = true;
                            }
                            break;

                        case "action":
                            output = HandleAction(output, gameDb, child, userInput);
                            break;

                        case "logic":
                            var lgc = new Parsers.Elements.Logic();

                            output = lgc.ParseLogic(output, gameDb, child, userInput);

                            if (output.FailedLogic)
                            {
                                output.FailedLogic = false;
                                return output;
                            }
                            else
                            {
                                continue;
                            }

                        case "move":
                            if (output.MatchMade)
                            {
                                continue;
                            }

                            if (!handledMove)
                            {
                                var move = new Parsers.Elements.Move();
                                output = move.ParseMove(output, gameDb, currentElement, child, index, userInput);
                                handledMove = output.MatchMade;
                            }
                            break;

                        case "set":
                            var set = new Parsers.Elements.Set();
                            output = set.ParseSet(output, gameDb, currentElement, child, userInput);
                            if (output.MatchMade)
                            {
                                var setProcs = Tools.ProcFunctions.GetProcessStepsByType(child.ElementType);
                                foreach (var childProc in setProcs)
                                {
                                    output = ParseElement(output, gameDb, child, userInput, childProc);
                                }
                            }
                            break;

                        case "object":
                            var objectProcs = Tools.ProcFunctions.GetProcessStepsByType(child.ElementType);
                            foreach (var childProc in objectProcs)
                            {
                                output = ParseElement(output, gameDb, child, userInput, childProc, isEntering);
                            }
                            break;

                        case "npc":
                            var npcProcs = Tools.ProcFunctions.GetProcessStepsByType(child.ElementType);
                            foreach (var childProc in npcProcs)
                            {
                                output = ParseElement(output, gameDb, child, userInput, childProc, isEntering);
                            }
                            break;
                    }
                }
            }

            return output;
        }

        private Output HandleAction(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var action = new Parsers.Elements.Action();
            return action.ParseAction(output, gameDb, currentElement, userInput);
        }

        private int HandleRepeat(string gameDb, Classes.Element currentElement, List<Classes.Element> children, string rptType)
        {
            var output = 0;
            DataManagement.GameLogic.Element dbElem = new DataManagement.GameLogic.Element();
            var index = dbElem.GetRepeatIndex(gameDb, currentElement.ElementKey);

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

                case "attribute":
                    if (!currentElement.Logic.Equals(""))
                    {
                        var attribElem = dbElem.GetElementByKey(gameDb, currentElement.Logic);
                        try
                        {
                            output = Convert.ToInt32(attribElem.Output) - 1;
                        }
                        catch (Exception)
                        {
                            return 0;
                        }
                    }
                    break;

                default:
                    output = 0;
                    break;
            }

            currentElement.RepeatIndex = output;
            dbElem.SetElementField(gameDb, currentElement.ElementKey, "RepeatIndex", output.ToString(), false);
            return output;
        }

        private Output HandleMessage(Output output, string gameDb, Classes.Element currentElement, Classes.Element child, string userInput, bool allowRepeatOptions, bool isEntering)
        {
            var rptType = currentElement.Repeat;

            var msg = new Parsers.Elements.Message();
            var msgParent = currentElement;
            var msgElem = child;
            var usingChild = false;
            var index = 0;

            output.MatchMade = false;

            // For certain elements, move in a level
            if (Tools.AppSettingFunctions.GetRootArray("Config/NonMessageParentTypes.json").Contains(currentElement.ElementType) &&
                child.Output.Equals(""))
            {
                msgParent = child;
                allowRepeatOptions = true;
                rptType = msgParent.Repeat;
                usingChild = true;
            }

            if (!msgParent.Repeat.Equals(""))
            {
                index = HandleRepeat(gameDb, msgParent, msgParent.Children, rptType);
            }
            else
            {
                index = -1;
            }

            if (usingChild)
            {
                foreach (var c in msgParent.Children)
                {
                    if (c.ElementType.Equals("message") || c.ElementType.Equals("enter_message"))
                    {
                        output = msg.ParseMessage(output, gameDb, msgParent, c, allowRepeatOptions, index);
                        if (output.MatchMade)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                output = msg.ParseMessage(output, gameDb, msgParent, msgElem, allowRepeatOptions, index);
            }

            return output;
        }
    }
}
