using System;
using System.Collections.Generic;
using System.Data.Common;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public void ParseElement(Classes.Element currentElement, Classes.ElementProc procObj, bool isEntering = false)
        {
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();
            var index = -1;
            var handledInpput = false;
            var handledMessage = false;
            var handledMove = false;
            var handledSet = false;
            var handledAttack = false;

            foreach (var proc in procObj.ChildProcElements)
            {
                handledMessage = false;

                foreach (var child in currentElement.Children)
                {
                    if (Cache.GameCache.Game == null)
                    {
                        return;
                    }

                    handledMove = false;

                    if (!child.ElementType.Equals(proc))
                    {
                        continue;
                    }

                    switch (child.ElementType)
                    {
                        case "attack":
                            if (handledAttack)
                            {
                                continue;
                            }

                            var attackParser = new Parsers.Elements.Attack();
                            attackParser.ParseAttack(currentElement, child);
                            if (MainClass.output.MatchMade && !MainClass.output.OutputText.Equals(""))
                            {
                                handledAttack = true;
                                return;
                            }
                            break;

                        case "input":
                            if (MainClass.output.MatchMade && !MainClass.output.OutputText.Equals(""))
                            {
                                continue;
                            }

                            if (!handledInpput)
                            {
                                input.ParseInput(currentElement, child);
                                if (MainClass.output.MatchMade)
                                {
                                    return;
                                }
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

                            MainClass.output.MatchMade = false;
                            HandleMessage(currentElement, child, procObj.AllowRepeatOptions, isEntering);
                            if (MainClass.output.MatchMade || !MainClass.output.OutputText.Equals(""))
                            {
                                handledMessage = true;
                                MainClass.output.MatchMade = true;
                            }
                            break;

                        case "action":
                            HandleAction(child);
                            if (Cache.GameCache.Game == null)
                            {
                                return;
                            }
                            break;

                        case "logic":
                            var lgc = new Parsers.Elements.Logic();

                            lgc.ParseLogic(child);

                            if (MainClass.output.FailedLogic)
                            {
                                MainClass.output.FailedLogic = false;
                                return;
                            }
                            else
                            {
                                continue;
                            }

                        case "move":
                            if (MainClass.output.MatchMade)
                            {
                                continue;
                            }

                            if (handledMove)
                            {
                                continue;
                            }

                            var move = new Parsers.Elements.Move();
                            move.ParseMove(currentElement, child, index);

                            if (MainClass.output.MatchMade)
                            {
                                return;
                            }
                            break;

                        case "set":
                            var set = new Parsers.Elements.Set();
                            set.ParseSet(currentElement, child);
                            if (MainClass.output.MatchMade)
                            {
                                var setProcs = Tools.ProcFunctions.GetProcessStepsByType(child.ElementType);
                                foreach (var childProc in setProcs)
                                {
                                    ParseElement(child, childProc);
                                    if (MainClass.output.MatchMade)
                                    {
                                        continue;
                                    }
                                }
                            }
                            break;

                        case "object":
                            child.ParseElement(isEntering);
                            break;

                        case "npc":
                            child.ParseElement(isEntering);
                            break;
                    }
                }
            }

            return;
        }

        private void HandleAction(Classes.Element currentElement)
        {
            var action = new Parsers.Elements.Action();
            action.ParseAction(currentElement);
            return;
        }

        private int HandleRepeat(Classes.Element currentElement, List<Classes.Element> children, string rptType)
        {
            var repeatOutput = 0;
            DataManagement.GameLogic.Element dbElem = new DataManagement.GameLogic.Element();
            var index = dbElem.GetRepeatIndex(currentElement.ElementKey);

            if (children.Count < 2)
            {
                return 0;
            }

            switch (rptType.Trim().ToLower())
            {
                case "none":
                    if (index == children.Count)
                    {
                        repeatOutput = children.Count - 1;
                    }
                    else
                    {
                        index++;
                        repeatOutput = index;
                    }
                    break;

                case "random":
                    if (index == -1)
                    {
                        var rnd = new Random((int)DateTime.Now.Ticks);
                        repeatOutput = rnd.Next(0, children.Count - 1);
                    }
                    else
                    {
                        repeatOutput = index;
                    }
                    break;

                case "repeat":
                    if (index >= children.Count - 1)
                    {
                        repeatOutput = 0;
                    }
                    else
                    {
                        index++;
                        repeatOutput = index;
                    }
                    break;

                case "attribute":
                    if (!currentElement.Logic.Equals(""))
                    {
                        var attribElem = dbElem.GetElementByKey(currentElement.Logic);
                        try
                        {
                            repeatOutput = Convert.ToInt32(attribElem.Output) - 1;
                        }
                        catch (Exception)
                        {
                            return 0;
                        }
                    }
                    break;

                default:
                    repeatOutput = 0;
                    break;
            }

            currentElement.RepeatIndex = repeatOutput;
            dbElem.SetElementField(currentElement.ElementKey, "RepeatIndex", repeatOutput.ToString(), false);
            return repeatOutput;
        }

        private void HandleMessage(Classes.Element currentElement, Classes.Element child, bool allowRepeatOptions, bool isEntering)
        {
            var rptType = currentElement.Repeat;

            var msg = new Parsers.Elements.Message();
            var msgParent = currentElement;
            var msgElem = child;
            var usingChild = false;
            var index = 0;

            MainClass.output.MatchMade = false;

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
                index = HandleRepeat(msgParent, msgParent.Children, rptType);
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
                        msg.ParseMessage(msgParent, c, allowRepeatOptions, index);
                        if (MainClass.output.MatchMade)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                msg.ParseMessage(msgParent, msgElem, allowRepeatOptions, index);
            }

            return;
        }
    }
}
