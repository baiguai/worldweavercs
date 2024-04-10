using System;
using System.Diagnostics.Tracing;
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
                var targetElement = elemDb.GetElementByKey(gameDb, arr[0].Trim());

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
                    newValue = newValue.Replace(currentElement.Tags, "").Trim();
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
    }
}
