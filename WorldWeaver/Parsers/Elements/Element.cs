using System.Runtime.InteropServices.Marshalling;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Element
    {
        public Random rnd = new Random(DateTime.Now.Millisecond);

        public void ParseElement(
            Classes.Element currentElement,
            Classes.ElementProc procObj,
            bool isEntering = false
        )
        {
            if (currentElement.ElementType.Equals("attribute", StringComparison.OrdinalIgnoreCase) ||
                currentElement.ElementType.Equals("attrib", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var input = new Parsers.Elements.Input();
            var msg = new Parsers.Elements.Message();
            var index = -1;
            var handledInput = false;
            var handledMessage = false;
            var handledMove = false;
            var handledAttack = false;
            var handledNav = false;

            foreach (var proc in procObj.ChildProcElements)
            {
                if (MainClass.output.FailedLogic)
                {
                    return;
                }
                handledMessage = false;
                handledMove = false;
                MainClass.output.FailedLogic = false; // @todo update how failed logic is processed.

                foreach (var child in currentElement.Children.Where(c => c.ElementType != "attribute" && c.ElementType != "attrib"))
                {
                    if (Tools.Elements.FailedLogicResetTypes().Contains(child.ElementType))
                    {
                        MainClass.output.FailedLogic = false;
                    }
                    if (MainClass.output.FailedLogic ||
                        handledMove)
                    {
                        continue;
                    }
                    if (child.ElementType.Equals("attribute", StringComparison.OrdinalIgnoreCase) ||
                        child.ElementType.Equals("attrib", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (currentElement.ElementType.Equals("room", StringComparison.OrdinalIgnoreCase) && !Cache.RoomCache.Room.ElementKey.Equals(currentElement.ElementKey))
                    {
                        return;
                    }

                    if (Cache.GameCache.Game == null)
                    {
                        return;
                    }


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
                            if (
                                MainClass.output.MatchMade
                                && !MainClass.output.OutputText.Equals("")
                            )
                            {
                                handledAttack = true;
                                return;
                            }
                            break;

                        case "input":
                            if (
                                currentElement.Active.Equals("true")
                                && MainClass.output.MatchMade
                                && !MainClass.output.OutputText.Equals("")
                            )
                            {
                                handledInput = true;
                                continue;
                            }

                            if (!handledInput)
                            {
                                input.ParseInput(currentElement, child);
                                if (MainClass.output.MatchMade || MainClass.output.FailedLogic)
                                {
                                    continue;
                                }
                            }
                            break;

                        case "message":
                        case "enter_message":
                            if (!currentElement.Active.Equals("true") ||
                                !child.Active.Equals("true"))
                            {
                                continue;
                            }
                            if (child.ElementType.Equals("enter_message", StringComparison.OrdinalIgnoreCase) && !isEntering)
                            {
                                continue;
                            }
                            if (handledMessage)
                            {
                                continue;
                            }

                            MainClass.output.MatchMade = false;
                            var allowRep = procObj.AllowRepeatOptions;
                            if (!currentElement.Repeat.Equals(""))
                            {
                                allowRep = true;
                            }
                            HandleMessage(
                                currentElement,
                                child,
                                allowRep,
                                isEntering
                            );
                            if (
                                MainClass.output.MatchMade
                                || !MainClass.output.OutputText.Equals("")
                            )
                            {
                                handledMessage = true;
                                MainClass.output.MatchMade = true;
                            }
                            break;

                        case "action":
                            if (!currentElement.Active.Equals("true"))
                            {
                                continue;
                            }
                            HandleAction(child);
                            if (Cache.GameCache.Game == null)
                            {
                                return;
                            }
                            break;

                        case "logic":
                            if (!currentElement.Active.Equals("true"))
                            {
                                continue;
                            }
                            var lgc = new Parsers.Elements.Logic();

                            lgc.ParseLogic(child);

                            if (MainClass.output.FailedLogic)
                            {
                                MainClass.output.MatchMade = true;
                                continue;
                            }
                            continue;

                        case "move":
                            if (!currentElement.Active.Equals("true"))
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
                        case "preset":
                            if (!currentElement.Active.Equals("true"))
                            {
                                continue;
                            }
                            var set = new Parsers.Elements.Set();
                            set.ParseSet(currentElement, child);
                            if (MainClass.output.MatchMade)
                            {
                                var setProcs = Tools.ProcFunctions.GetProcessStepsByType(
                                    child.ElementType
                                );
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
                            if (!currentElement.Active.Equals("true"))
                            {
                                continue;
                            }
                            child.ParseElement(isEntering);

                            if (MainClass.output.OutputText.Equals(""))
                            {
                                var elemDb = new DataManagement.GameLogic.Element();
                                var objChildren = elemDb.GetElementByKey(child.ElementKey);
                                foreach (var c in objChildren.Children)
                                {
                                    if (c.ElementType.Equals("object"))
                                    {
                                        c.ParseElement(isEntering);
                                        if (!MainClass.output.OutputText.Equals(""))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "npc":
                            if (!currentElement.Active.Equals("true"))
                            {
                                continue;
                            }
                            child.ParseElement(isEntering);
                            break;

                        case "devnote":
                            if (!MainClass.adminEnabled || !child.ElementType.Equals("devnote", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var devnote = new Parsers.Elements.DevNote();
                            devnote.ParseDevNote(child);
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

        private int HandleRepeat(
            Classes.Element currentElement,
            List<Classes.Element> children,
            string rptType
        )
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
                    var cnt = children.Count;
                    // If the child count is only 2 - rnd will always pick 0, so increase it and check that
                    if (cnt == 2)
                    {
                        cnt = 11;
                    }
                    repeatOutput = rnd.Next(cnt);
                    if (children.Count == 2)
                    {
                        if (repeatOutput < 5)
                        {
                            repeatOutput = 0;
                        }
                        else
                        {
                            repeatOutput = 1;
                        }
                    }
                    break;

                case "repeat":
                    if (children.Count == 1)
                    {
                        return 0;
                    }
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
                case "attrib":
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
            dbElem.SetElementField(
                currentElement.ElementKey,
                "RepeatIndex",
                repeatOutput.ToString(),
                false
            );
            return repeatOutput;
        }

        private void HandleMessage(
            Classes.Element currentElement,
            Classes.Element child,
            bool allowRepeatOptions,
            bool isEntering
        )
        {
            var rptType = currentElement.Repeat;

            var msg = new Parsers.Elements.Message();
            var msgParent = currentElement;
            var msgElem = child;
            var usingChild = false;
            var index = 0;
            var elemLogic = new Parsers.Elements.Logic();

            MainClass.output.MatchMade = false;

            // For certain elements, move in a level
            if (
                Tools
                    .AppSettingFunctions.GetRootArray("Config/NonMessageParentTypes.json")
                    .Contains(currentElement.ElementType) && child.Output.Equals("")
            )
            {
                msgParent = child;
                allowRepeatOptions = true;
                rptType = msgParent.Repeat;
                usingChild = true;
            }

            if (!msgParent.Repeat.Equals(""))
            {
                index = HandleRepeat(msgParent, msgParent.Children.Where(c => c.ElementType.Equals("message")).ToList(), rptType);
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

            if (currentElement.ElementType.Equals("room") || currentElement.ElementType.Equals("look"))
            {
                foreach (var c in Cache.RoomCache.Room.Children)
                {
                    if (c.ElementType.Equals("navigation"))
                    {
                        foreach (var msgChild in c.Children.Where(c => c.ElementType.Equals("message")))
                        {
                            msg.ParseMessage(c, msgChild, false, index);
                        }
                    }
                }
            }

            return;
        }
    }
}
