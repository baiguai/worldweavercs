using System;
using System.Diagnostics.Tracing;
using WorldWeaver.Tools;
namespace WorldWeaver.Parsers.Elements
{
    public class Set
    {
        public void ParseSet(Classes.Element parentElement, Classes.Element currentElement)
        {
            SetElementValue(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            SetElementChildValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            SetRelativeElementValue(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            SetRelativeElementChildValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);
            SetRelativeElementChildrenValueByTag(currentElement, currentElement.Tags, currentElement.Logic, currentElement.Output);

            return;
        }

        private void SetElementValue(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("(") || !logic.Contains(")"))
            {
                return;
            }

            if (logic.EndsWith(")"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split(")");
            if (arr.Length != 2)
            {
                return;
            }

            var prop = arr[1].Trim();
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");
            var elemDb = new DataManagement.GameLogic.Element();

            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return;
            }

            elemDb.SetElementField(key, prop, output.Trim());
        }

        private void SetElementChildValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))"))
            {
                return;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");

            var elemDb = new DataManagement.GameLogic.Element();
            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return;
            }
            var child = elem.Children.Where(c => c.Tags.TagsContain(tag)).FirstOrDefault();
            if (child == null || child.ElementKey.Equals(""))
            {
                return;
            }

            elemDb.SetElementField(child.ElementKey, prop, output.Trim());
        }

        private void SetRelativeElementValue(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("[") || !logic.Contains("]"))
            {
                return;
            }

            if (logic.EndsWith("]"))
            {
                logic = logic = "output";
            }

            var arr = logic.Split("]");
            if (arr.Length != 2)
            {
                return;
            }

            var prop = arr[1].Trim();
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var elem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (elem.ElementKey.Equals(""))
            {
                return;
            }

            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetElementField(elem.ElementKey, prop, output.Trim());
        }

        private void SetRelativeElementChildValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))") || !logic.Contains("[") || !logic.Contains("]"))
            {
                return;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return;
            }
            var targetElem = parentElem.Children.Where(c => c.Tags.TagsContain(tags)).First();
            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetElementField(targetElem.ElementKey, prop, output.Trim());
        }

        private void SetRelativeElementChildrenValueByTag(Classes.Element currentElement, string tags, string logic, string output)
        {
            if (!logic.Contains("((") || !logic.Contains("))") || !logic.Contains("[") || !logic.Contains("]"))
            {
                return;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return;
            }
            var targetElems = parentElem.Children.Where(c => c.Tags.TagsContain(tags));
            var elemDb = new DataManagement.GameLogic.Element();
            foreach (var child in targetElems)
            {
                elemDb.SetElementField(child.ElementKey, prop, output);
            }
        }
    }
}
