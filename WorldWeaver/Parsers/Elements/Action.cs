using System;
using System.Formats.Asn1;
using System.Globalization;
using WorldWeaver.Cache;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Action
    {
        internal void ParseAction(Classes.Element currentElement)
        {
            var elem = new Parsers.Elements.Element();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);

            foreach (var child in currentElement.Children)
            {
                foreach (var proc in procItems)
                {
                    elem.ParseElement(currentElement, proc);
                }
                if (MainClass.output.MatchMade)
                {
                    break;
                }
            }

            ParseMessageActions(currentElement);
            ParseLogicActions(currentElement);
        }

        internal void ParseMessageActions(Classes.Element currentElement)
        {
            if (currentElement.Tags.TagsContain("list"))
            {
                ParseTags_List(currentElement);
            }

            if (currentElement.Tags.TagsContain("type"))
            {
                ParseTags_Type(currentElement, currentElement.Logic);
            }

            if (currentElement.Tags.TagsContain("key"))
            {
                ParseTags_Key(currentElement.Logic);
            }

            return;
        }

        private void ParseTags_Type(Classes.Element currentElement, string type)
        {
            if (Cache.RoomCache.Room == null || MainClass.output.MatchMade)
            {
                return;
            }

            if (type.Equals("enter_message"))
            {
                var moveDb = new DataManagement.GameLogic.Move();
                moveDb.MoveElement("", Cache.PlayerCache.Player.ElementKey, Cache.PlayerCache.Player.ParentKey);
                return;
            }

            var self = Tools.Elements.GetSelf(currentElement);
            var targets = Tools.Elements.GetElementsByType(Cache.RoomCache.Room, type);
            var elemDb = new DataManagement.GameLogic.Element();

            var elemParser = new Parsers.Elements.Element();

            foreach (var elem in targets)
            {
                if (elem.ParentKey == self.ElementKey)
                {
                    var selfProcItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                    foreach (var proc in selfProcItems)
                    {
                        elemParser.ParseElement(elem, proc);
                    }
                }
            }

            foreach (var elem in targets)
            {
                if (elem.ParentKey != self.ElementKey)
                {
                    if ((!elem.Tags.TagsContain("inventory") &&
                        !Tools.Elements.GetSelf(elem).Tags.TagsContain("inventory")) ||
                        elem.Tags.TagsContain("inspect"))
                    {
                        var parent = elemDb.GetElementByKey(elem.ParentKey);
                        var procItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                        foreach (var proc in procItems)
                        {
                            elemParser.ParseElement(elem, proc);
                        }
                    }
                }
            }

            if (MainClass.output.OutputText.Equals(""))
            {
                ParseTags_Type(Cache.RoomCache.Room, type);
            }

            return;
        }

        private void ParseTags_List(Classes.Element currentElement)
        {
            var arr = currentElement.Logic.Split("((");

            if (arr.Length == 2)
            {
                var key = arr[0];
                Classes.Element elem = null;

                var logic = new DataManagement.GameLogic.Element();

                if (key.Equals("[self]"))
                {
                    elem = Tools.Elements.GetSelf(currentElement);
                }
                else
                {
                    elem = logic.GetElementByKey(key);
                }

                if (!currentElement.Output.Equals(""))
                {
                    MainClass.output.OutputText += $"{currentElement.Output}{Environment.NewLine}{Environment.NewLine}";
                }

                foreach (var child in elem.Children)
                {
                    if (child.Tags.TagsContain(arr[1].Replace("))", "").Trim()))
                    {
                        if (child.Output.Equals(""))
                        {
                            MainClass.output.OutputText += $"{child.Name}{Environment.NewLine}";
                        }
                        else
                        {
                            if (child.Tags.TagsContain("name"))
                            {
                                var elemDb = new DataManagement.GameLogic.Element();
                                var tgtElem = elemDb.GetElementByKey(child.Output);
                                if (!tgtElem.ElementKey.Equals(""))
                                {
                                    MainClass.output.OutputText += $"{child.Name}: {tgtElem.Name}{Environment.NewLine}";
                                }
                                else
                                {
                                    MainClass.output.OutputText += $"{child.Name}: {child.Output}{Environment.NewLine}";
                                }
                            }
                            else
                            {
                                MainClass.output.OutputText += $"{child.Name}: {child.Output}{Environment.NewLine}";
                            }
                        }
                        MainClass.output.MatchMade = true;
                    }
                }
            }

            return;
        }

        private void ParseTags_Key(string logic)
        {
            var thekey = logic;
            var outputSrc = "output";
            var outputText = "";

            var outList = Tools.LogicFunctions.GetParentAndField(logic, "(", ")");
            if (outList.Count() == 2)
            {
                thekey = outList[0];
                outputSrc = outList[1];
            }

            var elemDb = new DataManagement.GameLogic.Element();
            var tgtElem = elemDb.GetElementByKey(thekey);
            if (!tgtElem.ElementKey.Equals(""))
            {
                switch (outputSrc)
                {
                    case "name":
                        outputText = tgtElem.Name;
                        break;

                    case "logic":
                        outputText = tgtElem.Logic;
                        break;

                    default:
                        outputText = tgtElem.Output;
                        break;
                }
            }

            if (!outputText.Equals(""))
            {
                MainClass.output.OutputText = outputText;
                MainClass.output.MatchMade = true;
            }
        }


        private void ParseLogicActions(Classes.Element currentElement)
        {
            switch (currentElement.Logic.ToLower())
            {
                case "[die]":
                    DoDie();
                    return;
            }

            return;
        }

        public void DoDie() // @todo Figure out how to get this to be less buggy.
        {
            var msgParser = new Parsers.Elements.Message();
            var elemDb = new DataManagement.GameLogic.Element();
            var dieMsg = elemDb.GetElementByKey("die_message");

            if (dieMsg.ElementKey.Equals(""))
            {
                MainClass.output.OutputText = Tools.InitFunctions.GetInitMessage(false);
            }
            else
            {
                MainClass.output.OutputText = $"{dieMsg.Output}{Environment.NewLine}{Environment.NewLine}{Tools.InitFunctions.GetInitMessage(false)}";
            }

            Tools.Game.ClearEverything();

            return;
        }

        internal void DoKill()
        {
            var gameLgc = new DataManagement.GameLogic.Element();
            var dieElem = Cache.FightCache.Fight.Enemy.ChildByType("kill");
            var elemParser = new Parsers.Elements.Element();
            var procs = Tools.ProcFunctions.GetProcessStepsByType("kill");

            foreach (var proc in procs)
            {
                elemParser.ParseElement(dieElem, proc);
            }

            gameLgc.SetElementField(Cache.FightCache.Fight.Enemy.ElementKey, "ParentKey", "limbo");
            gameLgc.SetElementField(Cache.FightCache.Fight.Enemy.ElementKey, "Active", "false");
            Cache.FightCache.Fight = null;

            return;
        }
    }
}
