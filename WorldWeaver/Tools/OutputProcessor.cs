using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class OutputProcessor
    {
        public static string ProcessOutputText(string output, Classes.Element currentElement)
        {
            output = ProcessSpecialValues(currentElement, output);
            output = ProcessOutputLogic(output, currentElement);

            return output;
        }

        public static string ProcessSpecialValues(Classes.Element currentElement, string output)
        {
            var startPos = 0;
            var endPos = 0;
            var specialString = "";
            var specialStringReplc = "";
            var newValue = "";

            startPos = output.IndexOf("<<");

            while (startPos >= 0)
            {
                endPos = output.IndexOf(">>", startPos) + 2;
                specialString = output.Substring(startPos, (endPos - startPos));
                newValue = ProcessSpecialValue(currentElement, specialString);
                output = output.Replace(specialString, newValue);
                startPos = output.IndexOf("<<");
            }

            return output;
        }

        public static string ProcessSpecialValue(Element currentElement, string specialString)
        {
            var updated = specialString;
            var selElem = currentElement;

            if (updated.Equals("[player]"))
            {
                return Cache.PlayerCache.Player.ElementKey;
            }
            if (updated.Equals("[room]"))
            {
                return Cache.RoomCache.Room.ElementKey;
            }
            if (updated.Equals("[enemy]") && Cache.FightCache.Fight != null)
            {
                return Cache.FightCache.Fight.Target.ElementKey;
            }
            if (updated.Equals("[syntax]"))
            {
                return GetRoomChildKeyBySyntax(currentElement, MainClass.userInput);
            }

            var referencedElem = Tools.Elements.GetRelativeElement(currentElement, specialString);
            if (!referencedElem.ElementKey.Equals(""))
            {
                selElem = referencedElem;
            }

            updated = GetSpecialValue(selElem, specialString);
            if (!updated.Equals(specialString))
            {
                return updated;
            }
            updated = GetElementChildByTag(selElem, specialString);
            if (!updated.Equals(specialString))
            {
                return updated;
            }
            updated = GetElementPropertyByKey(selElem, specialString);
            if (!updated.Equals(specialString))
            {
                return updated;
            }
            updated = GetRelativeElementChildByTag(selElem, specialString);
            if (!updated.Equals(specialString))
            {
                return updated;
            }
            updated = GetRelativeElementProperty(selElem, specialString);
            if (!updated.Equals(specialString))
            {
                return updated;
            }

            return updated.Replace("<<", "").Replace(">>", "");
        }

        private static string GetSpecialValue(Element currentElement, string specialString)
        {
            if (specialString.Equals("<<[isday]>>"))
            {
                return Tools.Game.IsDay().ToString().ToLower();
            }

            if (specialString.Equals("<<[missiondays]>>"))
            {
                return Tools.Game.MissionDays().ToString().ToLower();
            }

            if (specialString.Equals("<<[totaldays]>>"))
            {
                return Tools.Game.TotalDays().ToString().ToLower();
            }

            return specialString;
        }

        private static string GetElementPropertyByKey(Element currentElement, string specialString)
        {
            if (!specialString.Contains("(") || !specialString.Contains(")"))
            {
                return specialString;
            }

            var arr = specialString.Replace("<<", "").Replace(">>", "").Split(")");
            var key = arr[0].Replace("(", "");
            var prop = "output";
            if (arr.Length == 2 && !arr[1].Trim().Equals(""))
            {
                prop = arr[1].Trim();
            }

            var elemDb = new DataManagement.GameLogic.Element();
            var targetElem = elemDb.GetElementByKey(key);
            if (targetElem.ElementKey.Equals(""))
            {
                return specialString;
            }
            var propValue = Tools.Elements.GetElementProperty(targetElem, prop);

            return propValue;
        }

        private static string GetElementChildByTag(Element currentElement, string specialString)
        {
            if (!specialString.Contains("((") || !specialString.Contains("))"))
            {
                return specialString;
            }

            var arr = specialString.Replace("<<", "").Replace(">>", "").Split("))");
            var processingString = arr[0].Replace("((", "");
            var prop = "output";
            if (arr.Length == 2 && !arr[1].Trim().Equals(""))
            {
                prop = arr[1].Trim();
            }
            arr = processingString.Split(")");
            if (arr.Length != 2 || arr[1].Trim().Equals(""))
            {
                return specialString;
            }
            var parentKey = arr[0].Trim().Replace("(", "");
            var tag = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var childElem = elemDb
                .GetElementChildren(parentKey)
                .Where(t => t.Tags.TagsContain(tag))
                .FirstOrDefault();
            if (childElem == null || childElem.ElementKey.Equals(""))
            {
                return specialString;
            }

            return Tools.Elements.GetElementProperty(childElem, prop);
        }

        private static string GetRelativeElementProperty(
            Element currentElement,
            string specialString
        )
        {
            if (!specialString.Contains("[") || !specialString.Contains("]"))
            {
                return specialString;
            }

            var arr = specialString.Replace("<<", "").Replace(">>", "").Split("]");
            var relCode = arr[0].Replace("[", "");
            var prop = "output";
            if (arr.Length == 2 && !arr[1].Trim().Equals(""))
            {
                prop = arr[1].Trim();
            }

            var targetElem = Tools.Elements.GetRelativeElement(currentElement, $"[{relCode}]");
            if (targetElem.ElementKey.Equals(""))
            {
                return specialString;
            }
            var propValue = Tools.Elements.GetElementProperty(targetElem, prop);

            return propValue;
        }

        private static string GetRelativeElementChildByTag(
            Element currentElement,
            string specialString
        )
        {
            if (!specialString.Contains("((") || !specialString.Contains("))"))
            {
                return specialString;
            }

            var arr = specialString.Replace("<<", "").Replace(">>", "").Split("))");
            var processingString = arr[0].Replace("((", "");
            var prop = "output";
            if (arr.Length == 2 && !arr[1].Trim().Equals(""))
            {
                prop = arr[1].Trim();
            }
            arr = processingString.Split("]");
            if (arr.Length != 2 || arr[1].Trim().Equals(""))
            {
                return specialString;
            }
            var parentRelCode = arr[0].Trim().Replace("[", "");
            var parentElement = Tools.Elements.GetRelativeElement(
                currentElement,
                $"[{parentRelCode}]"
            );
            if (parentElement == null || parentElement.ElementKey.Equals(""))
            {
                return specialString;
            }
            var tag = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var childElem = elemDb
                .GetElementChildren(parentElement.ElementKey)
                .Where(t => t.Tags.TagsContain(tag))
                .FirstOrDefault();
            if (childElem == null || childElem.ElementKey.Equals(""))
            {
                return specialString;
            }

            return Tools.Elements.GetElementProperty(childElem, prop);
        }

        private static string GetRoomChildKeyBySyntax(Classes.Element currentElement, string specialString)
        {
            var children = Cache.RoomCache.Room.Children;

            foreach (var child in children)
            {
                if (child.Syntax.Equals(""))
                {
                    continue;
                }

                var syntax = ProcessSpecialValue(child, child.Syntax);

                Regex rgx = new Regex(syntax, RegexOptions.IgnoreCase);
                if (rgx.IsMatch(specialString))
                {
                    // Return the first match found.
                    // If there are more than one - you will need to adjust your game definition.
                    return child.ElementKey;
                }
            }

            return "";
        }

        private static string ProcessOutputLogic(string output, Classes.Element currentElement)
        {
            var startPos = 0;
            var endPos = 0;

            startPos = output.IndexOf("<?");

            while (startPos >= 0)
            {
                // endPos = output.LastIndexOf("</?>") + 3;
                endPos = GetEndPos(output, startPos) + 3;
                var replaceBlock = output.SubstringByIndexes(startPos, endPos);
                var lgcBlock = replaceBlock.SubstringByIndexes(2, replaceBlock.Length - 5);
                var newValue = "";
                var arr = lgcBlock.Split("?>", 2);
                if (arr.Length != 2)
                {
                    output = output.Replace(
                        lgcBlock,
                        "!! SOMETHING WENT WRONG IN THE COMPARISON LOGIC !!"
                    );
                }
                var compareBlock = arr[0].Trim();
                var contentBlock = arr[1];
                var logicParse = new Parsers.Elements.Logic();

                if (logicParse.ParseConditional(currentElement, compareBlock))
                {
                    newValue = contentBlock;
                }

                output = output.Replace(replaceBlock, newValue);

                startPos = output.IndexOf("<?");
            }

            return output.RandomValue(currentElement);
        }

        private static int GetEndPos(string output, int startPos)
        {
            var endPosOut = 0;
            var snippet = "";
            var childCount = 0;
            var startTmp = startPos + 2;

            endPosOut = output.IndexOf("</?>");

            while (startTmp < endPosOut)
            {
                snippet = output.SubstringByIndexes(startTmp, endPosOut + 3);
                if (snippet.Contains("<?"))
                {
                    childCount++;
                    startTmp = output.IndexOf("<?", startTmp) + 2;
                }
                else
                {
                    break;
                }
            }

            endPosOut = output.IndexOf("</?>");

            if (childCount == 0)
            {
                return endPosOut;
            }

            for (var c = 0; c <= childCount; c++)
            {
                endPosOut = output.IndexOf("</?>", endPosOut) + 3;
            }

            return endPosOut;
        }
    }
}
