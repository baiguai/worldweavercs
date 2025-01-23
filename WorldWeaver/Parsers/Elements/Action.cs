using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
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
            var elemLogic = new Parsers.Elements.Logic();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);

            foreach (var proc in procItems)
            {
                if (proc.ChildProcElements.Contains("logic"))
                {
                    foreach (var child in currentElement.Children.Where(c => c.ElementType.Equals("logic", StringComparison.OrdinalIgnoreCase)))
                    {
                        elemLogic.ParseLogic(child);
                        if (MainClass.output.FailedLogic)
                        {
                            return;
                        }
                    }
                }
                foreach (var child in currentElement.Children)
                {
                    elem.ParseElement(currentElement, proc);
                    // @!@
                    // Added the global consideration.
                    // Global processing doesn't set MatchMade - so that other elements can still process.
                    if (MainClass.output.MatchMade ||
                        Tools.Elements.GetSelf(currentElement).ElementType.Equals("global", StringComparison.CurrentCultureIgnoreCase))
                    {
                        break;
                    }
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
                ParseTags_Type(currentElement, currentElement.Logic, currentElement.Tags);
            }

            if (currentElement.Tags.TagsContain("key"))
            {
                ParseTags_Key(currentElement.Logic);
            }

            if (currentElement.Tags.TagsContain("spawn"))
            {
                ParseTags_Spawn(currentElement);
            }

            return;
        }

        private void ParseTags_Type(Classes.Element currentElement, string type, string tags)
        {
            var level = GetTypeLevel(tags);
            var lvlOrder = AppSettingFunctions.GetRootArray("Config/ActionLevelOrder.json");

            if (Cache.RoomCache.Room == null || MainClass.output.MatchMade)
            {
                return;
            }

            if (type.Equals("enter_message"))
            {
                var moveDb = new DataManagement.GameLogic.Move();
                moveDb.MoveElement(currentElement, "", Cache.PlayerCache.Player.ElementKey, Cache.PlayerCache.Player.ParentKey);
                return;
            }

            if (level.Equals("self"))
            {
                if (Tools.Elements.GetSelf(currentElement).ParentKey.Equals(PlayerCache.Player.ElementKey))
                {
                    level = "player";
                }
            }

            foreach (var lvl in lvlOrder)
            {
                if (lvl.Equals(level))
                {
                    switch (level)
                    {
                        case "object":
                            ParseObject_Type(currentElement, type);
                            return;

                        case "room":
                            ParseRoom_Type(currentElement, type);
                            return;

                        case "self":
                            ParseSelf_Type(currentElement, type);
                            return;

                        case "player":
                            ParsePlayer_Type(type);
                            return;

                        case "global":
                            ParseGlobal_Type(currentElement, type);
                            return;
                    }
                }
            }

            return;
        }

        private void ParseObject_Type(Classes.Element currentElement, string type)
        {
            var logic = currentElement.Logic;
            if (currentElement.Logic.StartsWith("["))
            {
                var arr = currentElement.Logic.Split("]");
                if (arr.Length == 2)
                {
                    type = arr[1].Trim();
                    logic = arr[0].Replace("[", "");
                }
            }
            var selfElem = Tools.Elements.GetRelativeElement(currentElement, logic);
            var targets = Tools.Elements.GetElementsByType(selfElem, type, true);
            var elemParser = new Parsers.Elements.Element();

            foreach (var elem in targets)
            {
                var selfProcItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                foreach (var proc in selfProcItems)
                {
                    elemParser.ParseElement(elem, proc);
                }
            }
        }

        private void ParsePlayer_Type(string type)
        {
            var selfElem = Cache.PlayerCache.Player;
            var targets = Tools.Elements.GetElementsByType(selfElem, type, true);
            var elemParser = new Parsers.Elements.Element();

            foreach (var elem in targets)
            {
                var selfProcItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                foreach (var proc in selfProcItems)
                {
                    elemParser.ParseElement(elem, proc);
                }
            }
        }

        private void ParseSelf_Type(Classes.Element currentElement, string type)
        {
            var selfElem = Tools.Elements.GetSelf(currentElement);

            ParseChildren(selfElem, type, true);
        }

        private void ParseRoom_Type(Classes.Element currentElement, string type)
        {
            ParseChildren(RoomCache.Room, type, true);
        }

        private void ParseGlobal_Type(Classes.Element currentElement, string type)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var globalElems = elemDb.GetElementsByType("global");

            foreach (var globalElem in globalElems)
            {
                var targets = Tools.Elements.GetElementsByType(globalElem, type, true);
                var elemParser = new Parsers.Elements.Element();

                foreach (var elem in targets)
                {
                    var selfProcItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                    foreach (var proc in selfProcItems)
                    {
                        elemParser.ParseElement(elem, proc);
                    }
                }

                foreach (var child in currentElement.GetChildren())
                {
                    ParseChildren(child, type);
                }
            }
        }

        private void ParseChildren(Classes.Element parentElement, string type, bool isRoom = false)
        {
            var targets = Tools.Elements.GetElementsByType(parentElement, type, true);
            foreach (var target in targets)
            {
                var targetProcs = Tools.ProcFunctions.GetProcessStepsByType(parentElement.ElementType);
                foreach (var proc in targetProcs)
                {
                    var elemParser = new Parsers.Elements.Element();
                    elemParser.ParseElement(target, proc);
                }
            }
        }

        private object GetTypeLevel(string tags)
        {
            var level = "room";

            if (tags.TagsContain("global"))
            {
                return "global";
            }
            if (tags.TagsContain("self"))
            {
                return "self";
            }
            if (tags.TagsContain("object"))
            {
                return "object";
            }

            return level;
        }

        private void ParseTags_List(Classes.Element currentElement)
        {
            var arr = currentElement.Logic.Split("((");

            if (arr.Length == 2)
            {
                var key = arr[0];
                Classes.Element elem = null;

                var logic = new DataManagement.GameLogic.Element();

                if (key.Equals("[self]", StringComparison.CurrentCultureIgnoreCase))
                {
                    elem = Tools.Elements.GetSelf(currentElement);
                }
                if (key.Equals("[player]", StringComparison.CurrentCultureIgnoreCase))
                {
                    elem = Cache.PlayerCache.Player;
                }
                if (key.Equals("[room]", StringComparison.CurrentCultureIgnoreCase))
                {
                    elem = Cache.RoomCache.Room;
                }
                if (key.Equals("[enemy]", StringComparison.CurrentCultureIgnoreCase) && Cache.FightCache.Fight != null)
                {
                    elem = Cache.FightCache.Fight.Enemies.First();
                }
                if (elem == null)
                {
                    elem = logic.GetElementByKey(key);
                }

                if (!currentElement.Output.Equals(""))
                {
                    MainClass.output.OutputText += $"{Tools.OutputProcessor.ProcessOutputText(currentElement.Output, currentElement)}{Environment.NewLine}{Environment.NewLine}";
                }

                foreach (var child in elem.Children)
                {
                    if (child.Tags.TagsContain(arr[1].Replace("))", "").Trim()))
                    {
                        if (child.Output.Equals(""))
                        {
                            MainClass.output.OutputText += $"{child.Name}{Environment.NewLine}";
                            MainClass.output.OutputText = MainClass.output.OutputText.RandomValue(currentElement);
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
                                    MainClass.output.OutputText += $"{child.Name}: {child.Output.RandomValue(child)}{Environment.NewLine}";
                                }
                            }
                            else
                            {
                                MainClass.output.OutputText += $"{child.Name}: {child.Output.RandomValue(child)}{Environment.NewLine}";
                            }
                        }

                        if (!Tools.Elements.GetSelf(child).ElementType.Equals("global", StringComparison.CurrentCultureIgnoreCase))
                        {
                            MainClass.output.MatchMade = true;
                        }
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
                MainClass.output.OutputText = Tools.OutputProcessor.ProcessOutputText(outputText, tgtElem);
                MainClass.output.MatchMade = true;
            }
        }

        private void ParseTags_Spawn(Classes.Element currentElement)
        {
            var spawn = new Parsers.Elements.Spawn();
            if (!currentElement.Logic.Equals(""))
            {
                spawn.SpawnByTemplate(currentElement);
            }
            else
            { }
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

        public void DoDie()
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
            if (Cache.FightCache.Fight == null ||
                MainClass.macro.IsRunning)
            {
                return;
            }
            var gameLgc = new DataManagement.GameLogic.Element();
            var dieElem = Cache.FightCache.Fight.Target.ChildByType("kill");
            var elemParser = new Parsers.Elements.Element();

            foreach (var child in dieElem.Children)
            {
                if (child.ElementType.Equals("action", StringComparison.CurrentCultureIgnoreCase))
                {
                    child.ParseElement();
                }
                if (child.ElementType.Equals("message", StringComparison.CurrentCultureIgnoreCase))
                {
                    MainClass.output.OutputText += child.Output;
                }
            }

            gameLgc.SetElementField(Cache.FightCache.Fight.Target.ElementKey, "ParentKey", "limbo");
            gameLgc.SetElementField(Cache.FightCache.Fight.Target.ElementKey, "Active", "false");
            Tools.CacheManager.RefreshCache();

            return;
        }
    }
}
