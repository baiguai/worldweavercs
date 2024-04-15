using System;
using System.Formats.Asn1;
using System.Globalization;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Action
    {
        internal Classes.Output ParseAction(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var elem = new Parsers.Elements.Element();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);

            output = ParseMessageActions(output, gameDb, currentElement, userInput);
            output = ParseLogicActions(output, gameDb, currentElement, userInput);

            foreach (var child in currentElement.Children)
            {
                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, currentElement, userInput, proc);
                }
                if (output.MatchMade)
                {
                    break;
                }
            }

            return output;
        }

        internal Classes.Output ParseMessageActions(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var tags = currentElement.Tags.Split('|');

            if (tags.Contains("list", StringComparer.OrdinalIgnoreCase))
            {
                output = ParseTags_List(output, gameDb, currentElement, userInput);
            }

            if (tags.Contains("type", StringComparer.OrdinalIgnoreCase))
            {
                output = ParseTags_Type(output, gameDb, currentElement, currentElement.Logic, userInput);
            }

            return output;
        }

        private Output ParseTags_Type(Output output, string gameDb, Classes.Element currentElement, string type, string userInput)
        {
            if (Cache.RoomCache.Room == null || output.MatchMade)
            {
                return output;
            }
            var self = Tools.Elements.GetSelf(gameDb, currentElement);
            var targets = Tools.Elements.GetElementsByType(Cache.RoomCache.Room, type);
            var elemDb = new DataManagement.GameLogic.Element();

            var elemParser = new Parsers.Elements.Element();
            
            foreach (var elem in targets)
            {
                if ((!elem.Tags.TagsContain("inventory") &&
                    !Tools.Elements.GetSelf(gameDb, elem).Tags.TagsContain("inventory")) || 
                    elem.Tags.TagsContain("inspect"))
                {
                    var parent = elemDb.GetElementByKey(gameDb, elem.ParentKey);
                    var procItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                    foreach (var proc in procItems)
                    {
                        output = elemParser.ParseElement(output, gameDb, elem, userInput, proc);
                    }
                }
            }

            if (output.OutputText.Equals(""))
            {
                output = ParseTags_Type(output, gameDb, Cache.RoomCache.Room, type, userInput);
            }

            return output;
        }

        private Output ParseTags_List(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var arr = currentElement.Logic.Split("((");

            if (arr.Length == 2)
            {
                var key = arr[0];
                Classes.Element elem = null;

                var logic = new DataManagement.GameLogic.Element();

                if (key.Equals("[self]"))
                {
                    elem = Tools.Elements.GetSelf(gameDb, currentElement);
                }
                else
                {
                    elem = logic.GetElementByKey(gameDb, key);
                }

                if (!currentElement.Output.Equals(""))
                {
                    output.OutputText += $"{currentElement.Output}{Environment.NewLine}{Environment.NewLine}";
                }

                foreach (var child in elem.Children)
                {
                    if (child.Tags.TagsContain(arr[1].Replace("))", "").Trim()))
                    {
                        if (child.Output.Equals(""))
                        {
                            output.OutputText += $"{child.Name}{Environment.NewLine}";
                        }
                        else
                        {
                            output.OutputText += $"{child.Name}: {child.Output}{Environment.NewLine}";
                        }
                        output.MatchMade = true;
                    }
                }
            }

            return output;
        }

        private Output ParseLogicActions(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            switch (currentElement.Logic.ToLower())
            {
                case "[die]":
                    var msgParser = new Parsers.Elements.Message();
                    var elemDb = new DataManagement.GameLogic.Element();
                    var dieMsg = elemDb.GetElementByKey(gameDb, "die_message");

                    if (dieMsg.ElementKey.Equals(""))
                    {
                        output.OutputText = Tools.InitFunctions.GetInitMessage(false);
                    }
                    else
                    {
                        output.OutputText = dieMsg.Output;
                    }

                    Tools.CacheManager.ClearCache();
                    output.MatchMade = true;
                    return output;
            }

            return output;
        }
    }
}
