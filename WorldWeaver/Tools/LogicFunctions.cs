using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class LogicFunctions
    {
        public static List<LogicElement> GetLogicValue(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            if (logicString.Equals("[self]"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "key",
                        Value = Tools.Elements.GetSelf(currentElement).ElementKey
                    }
                );
                return logicElems;
            }

            if (logicString.Equals("[output]"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "key",
                        Value = MainClass.userInput
                    }
                );
            }

            if (logicString.Equals("[room]"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "key",
                        Value = Cache.RoomCache.Room.ElementKey
                    }
                );
                return logicElems;
            }

            if (logicString.Equals("[player]"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "key",
                        Value = Cache.PlayerCache.Player.ElementKey
                    }
                );
                return logicElems;
            }

            if (logicString.Equals("[children]"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "count",
                        Value = Tools.Elements.GetSelf(currentElement).Children.Where(c => c.ElementType.Equals("object", StringComparison.OrdinalIgnoreCase)).ToList().Count.ToString()
                    }
                );
                return logicElems;
            }

            if (logicString.Equals("[enemy]"))
            {
                if (Cache.FightCache.Fight == null)
                {
                    return logicElems;
                }
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "key",
                        Value = Cache.FightCache.Fight.Target.ElementKey
                    }
                );
                return logicElems;
            }


            if (Regex.IsMatch(logicString.Trim(), @"^\d+$"))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "number",
                        Value = logicString.Trim()
                    }
                );
                return logicElems;
            }

            if (logicString.StartsWith("rdm_", StringComparison.CurrentCultureIgnoreCase))
            {
                logicElems.Add(
                    new LogicElement()
                    {
                        Property = "output",
                        Value = logicString.RandomValue(currentElement)
                    }
                );
                return logicElems;
            }


            var inputElem = ElementPropFromInput(currentElement, logicString);
            if (inputElem.Count > 0)
            {
                return inputElem;
            }

            var elemByKey = ParseElementByKey(currentElement, logicString);
            if (elemByKey.Count > 0)
            {
                return elemByKey;
            }

            var elemChildByTag = ParseElementChildByTag(currentElement, logicString);
            if (elemChildByTag.Count > 0)
            {
                return elemChildByTag;
            }

            var elemRelative = ParseRelativeElement(currentElement, logicString);
            if (elemRelative.Count > 0)
            {
                return elemRelative;
            }

            var relElemByTag = ParseRelativeElementByTag(currentElement, logicString);
            if (relElemByTag.Count > 0)
            {
                return relElemByTag;
            }

            var presetElem = ParsePresetVariables(currentElement, logicString);
            if (presetElem.Count > 0)
            {
                return presetElem;
            }

            if (logicString.StartsWith("ls("))
            {
                logicString = logicString.Replace("ls(", "(");
                var listByTag = ListElementChildrenByTag(currentElement, logicString);
                if (listByTag.Count > 0)
                {
                    return listByTag;
                }
            }

            if (logicString.StartsWith("ls["))
            {
                logicString = logicString.Replace("ls[", "[");
                var listRelByTag = ListRelativeElementChildrenByTag(currentElement, logicString);
                if (listRelByTag != null)
                {
                    return listRelByTag;
                }
            }

            return logicElems;
        }


        private static List<LogicElement> ElementPropFromInput(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();
            if (!logicString.Contains("[syntax]", StringComparison.OrdinalIgnoreCase))
            {
                return logicElems;
            }

            var prop = "output";
            var arr = logicString.Split("(");
            if (arr.Length == 2)
            {
                prop = arr[1].Remove(')');
            }

            var elemDb = new DataManagement.GameLogic.Element();

            var foundElems = elemDb.GetElementKeysBySyntax(MainClass.userInput);
            if (foundElems.Count != 1)
            {
                return null;
            }

            var foundElem = elemDb.GetElementByKey(foundElems.First());
            logicElems.Add(
                new LogicElement()
                {
                    Property = prop,
                    Value = Tools.Elements.GetElementProperty(foundElem, prop)
                }
            );
            return logicElems;
        }

        private static List<LogicElement> ParseElementByKey(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            if (logicString.EndsWith(")"))
            {
                logicString = logicString += "output";
            }
            var arr = logicString.Split(")");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var key = arr[0].Trim().Replace("(", "");
            var prop = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return logicElems;
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = prop,
                    Value = Tools.Elements.GetElementProperty(curElement, prop)
                }
            );
            return logicElems;
        }

        private static List<LogicElement>? ParseElementChildByTag(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();
            var propValue = "";
            if (logicString.EndsWith("))"))
            {
                logicString = logicString += "output";
            }

            var arr = logicString.Split("))");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var key = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return null;
            }
            var elemChildren = curElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                }
                propValue += Tools.Elements.GetElementProperty(ch, prop);
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = prop,
                    Value = propValue
                }
            );
            return logicElems;
        }

        private static List<LogicElement> ParseRelativeElement(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            if (logicString.EndsWith("]"))
            {
                logicString = logicString += "output";
            }

            var arr = logicString.Split("))");

            if (arr.Length == 2)
            {
                var outputValue = ParseRelativeElementByTag(currentElement, logicString);
                if (!outputValue.Equals(""))
                {
                    return logicElems;
                }
            }

            arr = logicString.Split("]");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var relCode = $"{arr[0].Trim()}]";
            var prop = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var curElement = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (curElement.ElementKey.Equals(""))
            {
                return logicElems;
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = prop,
                    Value = Tools.Elements.GetElementProperty(curElement, prop)
                }
            );
            return logicElems;
        }

        private static List<LogicElement> ParseRelativeElementByTag(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            if (logicString.EndsWith(")"))
            {
                logicString = logicString += "output";
            }
            var arr = logicString.Split("))");
            var prop = "";
            if (arr.Length == 2)
            {
                prop = arr[1].Trim();
            }
            arr = arr[0].Split("]");
            if (arr.Length != 2)
            {
                return logicElems;
            }
            var tag = arr[1].Trim().Replace("((", "");
            var rel = arr[0].Trim().Replace("[", "");
            rel = $"[{rel}]";

            var curElement = Tools.Elements.GetRelativeElement(currentElement, rel);
            var childElement = curElement.Children.Where(c => c.Tags.TagsContain(tag)).FirstOrDefault();

            if (childElement == null)
            {
                return logicElems;
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = prop,
                    Value = Tools.Elements.GetElementProperty(childElement, prop)
                }
            );
            return logicElems;
        }

        private static List<LogicElement> ParsePresetVariables(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            switch (logicString)
            {
                case "[isday]":
                    logicElems.Add(
                        new LogicElement()
                        {
                            Property = "output",
                            Value = Tools.Game.IsDay().ToString().ToLower()
                        }
                    );
                    return logicElems;

                case "[missiondays]":
                    logicElems.Add(
                        new LogicElement()
                        {
                            Property = "output",
                            Value = Tools.Game.MissionDays().ToString().ToLower()
                        }
                    );
                    return logicElems;

                case "[totaldays]":
                    logicElems.Add(
                        new LogicElement()
                        {
                            Property = "output",
                            Value = Tools.Game.TotalDays().ToString().ToLower()
                        }
                    );
                    return logicElems;

                default:
                    return logicElems;
            }
        }

        private static List<LogicElement> ListElementChildrenByTag(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            var propValue = "";
            if (logicString.EndsWith("))"))
            {
                logicString = logicString += "output";
            }

            var arr = logicString.Split("))");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var key = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return logicElems;
            }
            var elemChildren = curElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                    propValue += Tools.Elements.GetElementProperty(ch, prop);
                }
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = "output",
                    Value = propValue
                }
            );
            return logicElems;
        }

        private static List<LogicElement> ListRelativeElementChildrenByTag(Element currentElement, string logicString)
        {
            var logicElems = new List<LogicElement>();

            var propValue = "";
            if (logicString.EndsWith("))"))
            {
                logicString = logicString += "output";
            }

            var arr = logicString.Split("))");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return logicElems;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var relCode = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var relElement = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (relElement.ElementKey.Equals(""))
            {
                return logicElems;
            }
            var elemChildren = relElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                    propValue += Tools.Elements.GetElementProperty(ch, prop);
                }
            }

            logicElems.Add(
                new LogicElement()
                {
                    Property = "output",
                    Value = propValue
                }
            );
            return logicElems;
        }

        public static List<string> GetParentAndField(string logicString, string openString, string closeString)
        {
            var outputList = new List<string>();

            var logArr = logicString.Split(openString);
            if (logArr.Length == 2)
            {
                outputList.Add(logArr[0].Trim());
                outputList.Add(logArr[1].Replace(closeString, "").Trim());
                return outputList;
            }
            else
            {
                outputList.Add(logicString);
                return outputList;
            }
        }

    }
}