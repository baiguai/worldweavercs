using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class OutputProcessor
    {
        public static string ProcessOutputText(string output, Classes.Element currentElement)
        {
            output = ProcessSpecialValues(output, currentElement);
            output = ProcessOutputLogic(output, currentElement);

            return output;
        }

        public static string ProcessSpecialValues(string output, Classes.Element currentElement)
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
                newValue = GetNewValue(currentElement, specialString);
                output = output.Replace(specialString, newValue);
                startPos = output.IndexOf("<<");
            }

            return output;
        }

        private static string GetNewValue(Element currentElement, string specialString)
        {
            var updated = specialString;

            updated = GetSpecialValue(currentElement, specialString);
            if (updated.Contains("<<"))
            {
                updated = GetElementPropertyByKey(currentElement, specialString);
            }
            if (updated.Contains("<<"))
            {
                updated = GetElementChildByTag(currentElement, specialString);
            }
            if (updated.Contains("<<"))
            {
                updated = GetRelativeElementChildByTag(currentElement, specialString);
            }
            if (updated.Contains("<<"))
            {
                updated = GetRelativeElementProperty(currentElement, specialString);
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

            return output;
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
