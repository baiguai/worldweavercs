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
                Classes.Element targetElement = null;

                if (arr[0].Trim().Equals("[self]"))
                {
                    targetElement = Tools.Elements.GetSelf(gameDb, currentElement);
                }
                else
                {
                    targetElement = elemDb.GetElementByKey(gameDb, arr[0].Trim());
                }

                if (targetElement.ElementKey.Equals(""))
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

                if (targetField.Equals("tags"))
                {
                    newValue = NewTagsValue(targetElement, newValue);
                }

                elemDb.SetElementField(gameDb, targetElement.ElementKey, targetField, newValue);
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
