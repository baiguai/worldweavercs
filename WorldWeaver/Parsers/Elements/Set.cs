using System;
using System.Diagnostics.Tracing;
using WorldWeaver.Tools;
namespace WorldWeaver.Parsers.Elements
{
    public class Set
    {
        public Classes.Output ParseSet(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var logParse = new Parsers.Elements.Logic();
            var logic = currentElement.Logic;
            var targetField = "";
            var newValue = "";

            output.MatchMade = false;

            logic = logParse.ParseSetLogic(gameDb, logic, userInput);

            var arr = logic.Split('|');

            if (arr.Length == 3)
            {
                var targetElements = GetTargetElements(gameDb, currentElement, arr[0].Trim());

                if (targetElements.Count() < 1)
                {
                    output.OutputText += "An error occurred while setting a value.";
                    output.MatchMade = false;
                    return output;
                }

                targetField = arr[1].Trim();
                newValue = arr[2].Trim();
                if (!currentElement.Tags.Equals(""))
                {
                    // This allows the game to remove additional text if needed
                    newValue = newValue.Replace(currentElement.Tags, "").Trim();
                }

                foreach (var targetElement in targetElements)
                {
                    if (targetField.Equals("tags"))
                    {
                        newValue = NewTagsValue(targetElement, newValue);
                    }

                    elemDb.SetElementField(gameDb, targetElement.ElementKey, targetField, newValue);
                }
                output.MatchMade = true;
            }
            else
            {
                output.OutputText += "An error has occured while setting a value.";
                output.MatchMade = false;
                return output;
            }

            return output;
        }

        private List<Classes.Element> GetTargetElements(string gameDb, Classes.Element currentElement, string targetString)
        {
            var targetElements = new List<Classes.Element>();
            var elemDb = new DataManagement.GameLogic.Element();
            var key = "";
            var type = "";
            var tag = "";

            if (targetString.Equals("[self]"))
            {
                targetElements.Add(Tools.Elements.GetSelf(gameDb, currentElement));
                return targetElements;
            }

            var arr = targetString.Split("((");
            if (arr.Length == 2)
            {
                key = arr[0].Trim();
                tag = arr[1].Trim().Replace("))", "");

                var tmpElem = new Classes.Element();
                if (key.Equals("[self]"))
                {
                    tmpElem = Tools.Elements.GetSelf(gameDb, currentElement);
                }
                else
                {
                    tmpElem = elemDb.GetElementByKey(gameDb, key);
                }

                if (tmpElem.Tags.Contains(tag))
                {
                    targetElements.Add(tmpElem);
                }

                targetElements.AddRange(tmpElem.Children.Where(c => c.Tags.Contains(tag)));
                return targetElements;
            }

            arr = targetString.Split("(");
            if (arr.Length == 2)
            {
                key = arr[0].Trim();
                type = arr[1].Trim().Replace(")", "");

                var tmpElem = new Classes.Element();
                if (key.Equals("[self]"))
                {
                    tmpElem = Tools.Elements.GetSelf(gameDb, currentElement);
                }
                else
                {
                    tmpElem = elemDb.GetElementByKey(gameDb, key);
                }

                if (tmpElem.ElementType.Equals(type))
                {
                    targetElements.Add(tmpElem);
                }

                targetElements.AddRange(tmpElem.Children.Where(c => c.ElementType.Equals(type)));
                return targetElements;
            }

            targetElements.Add(elemDb.GetElementByKey(gameDb, targetString));

            return targetElements;
        }

        private string NewTagsValue(Classes.Element targetElement, string newValue)
        {
            var output = "";

            if (newValue.Length > 0)
            {
                switch (newValue.Substring(0, 1))
                {
                    case "+":
                        newValue = newValue.Replace("+", "");
                        output = targetElement.Tags.AddTag(newValue);
                        break;

                    case "-":
                        newValue = newValue.Replace("-", "");
                        output = targetElement.Tags.RemoveTag(newValue);
                        break;

                    case "*":
                        newValue = newValue.Replace("*", "");
                        var nvArr = newValue.Split('>');
                        if (nvArr.Length == 2)
                        {
                            output = targetElement.Tags.ReplaceTag(nvArr[0].Trim(), nvArr[1].Trim());
                        }
                        break;
                }
            }

            return output;
        }
    }
}
