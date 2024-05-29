using System;
using System.Diagnostics.Tracing;
using WorldWeaver.Tools;
namespace WorldWeaver.Parsers.Elements
{
    public class Set
    {
        public void ParseSet(Classes.Element parentElement, Classes.Element currentElement)
        {
            var setPerformed = false;

            SetElementValue(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            if (!setPerformed)
            {
                setPerformed = SetElementChildValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            }
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementChildValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            }
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementChildrenValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            }
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementValue(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            }

            return;
        }

        private bool SetElementValue(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("(") || !logic.Contains(")"))
            {
                return false;
            }

            if (logic.EndsWith(")"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split(")");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");
            var elemDb = new DataManagement.GameLogic.Element();

            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }

            elemDb.SetElementField(key, prop, output.Trim());
            return true;
        }

        private bool SetElementChildValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))"))
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");

            var elemDb = new DataManagement.GameLogic.Element();
            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }
            var child = elem.Children.Where(c => c.Tags.TagsContain(tag)).FirstOrDefault();
            if (child == null || child.ElementKey.Equals(""))
            {
                return false;
            }

            elemDb.SetElementField(child.ElementKey, prop, output.Trim());
            return true;
        }

        private bool SetRelativeElementValue(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("[") || !logic.Contains("]"))
            {
                return false;
            }

            if (logic.EndsWith("]"))
            {
                logic = logic = "output";
            }

            var arr = logic.Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var elem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }

            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetElementField(elem.ElementKey, prop, output.Trim());
            return true;
        }

        private bool SetRelativeElementChildValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))") || !logic.Contains("[") || !logic.Contains("]"))
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            if (tag.Equals(""))
            {
                tag = tags;
            }
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return false;
            }
            var targetElem = parentElem.Children.Where(c => c.Tags.TagsContain(tag)).First();
            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetElementField(targetElem.ElementKey, prop, output.Trim());
            return true;
        }

        private bool SetRelativeElementChildrenValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))") || !logic.Contains("[") || !logic.Contains("]"))
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return false;
            }
            var targetElems = parentElem.Children.Where(c => c.Tags.TagsContain(tags));
            var elemDb = new DataManagement.GameLogic.Element();
            foreach (var child in targetElems)
            {
                elemDb.SetElementField(child.ElementKey, prop, output);
            }

            return true;
        }
    }
}
