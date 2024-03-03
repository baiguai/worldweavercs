using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class SetField
    {
        public Classes.Output DoMatch(string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            var output = new Classes.Output();
            var logicArr = currentElement.Logic.Split('|');

            output.OutputText = "false";

            if (logicArr.Length == 2)
            {
                var elemKey = logicArr[0].Trim();
                var field = logicArr[1].Trim();
                var newValue = userInput.GetInputParams();

                var dbLogic = new DataManagement.GameLogic.Element();
                var success = dbLogic.SetElementField(gameDb, elemKey, field, newValue);

                if (success)
                {
                    Message msg = new Message();
                    foreach (var elem in parentElement.Children)
                    { 
                        if (elem.ElementType.Equals("message"))
                        {
                            output.OutputText = elem.Output.OutputFormat();
                        }
                    }
                }
            }

            return output;
        }
    }
}
