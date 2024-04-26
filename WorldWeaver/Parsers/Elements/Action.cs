using System;
using System.Formats.Asn1;
using System.Globalization;
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
            var tags = currentElement.Tags.Split('|');

            if (tags.Contains("list", StringComparer.OrdinalIgnoreCase))
            {
                ParseTags_List(currentElement);
            }

            if (tags.Contains("type", StringComparer.OrdinalIgnoreCase))
            {
                ParseTags_Type(currentElement, currentElement.Logic);
            }

            return;
        }

        private void ParseTags_Type(Classes.Element currentElement, string type)
        {
            if (Cache.RoomCache.Room == null || MainClass.output.MatchMade)
            {
                return;
            }
            var self = Tools.Elements.GetSelf(MainClass.gameDb, currentElement);
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
                        !Tools.Elements.GetSelf(MainClass.gameDb, elem).Tags.TagsContain("inventory")) ||
                        elem.Tags.TagsContain("inspect"))
                    {
                        var parent = elemDb.GetElementByKey(MainClass.gameDb, elem.ParentKey);
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
                    elem = Tools.Elements.GetSelf(MainClass.gameDb, currentElement);
                }
                else
                {
                    elem = logic.GetElementByKey(MainClass.gameDb, key);
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
                            MainClass.output.OutputText += $"{child.Name}: {child.Output}{Environment.NewLine}";
                        }
                        MainClass.output.MatchMade = true;
                    }
                }
            }

            return;
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
            var dieMsg = elemDb.GetElementByKey(MainClass.gameDb, "die_message");

            if (dieMsg.ElementKey.Equals(""))
            {
                MainClass.output.OutputText = Tools.InitFunctions.GetInitMessage(false);
            }
            else
            {
                MainClass.output.OutputText = dieMsg.Output;
            }

            Tools.CacheManager.ClearCache();
            MainClass.output.MatchMade = true;

            return;
        }

        internal void DoKill()
        {
            var dieElem = Cache.FightCache.Fight.Enemy.ChildByType("kill");
            var elemParser = new Parsers.Elements.Element();
            var procs = Tools.ProcFunctions.GetProcessStepsByType("kill");
            foreach (var proc in procs)
            {
                elemParser.ParseElement(dieElem, proc);
            }

            return;
        }
    }
}
