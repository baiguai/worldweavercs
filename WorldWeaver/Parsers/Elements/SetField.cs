using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class SetField
    {
        public Classes.Output xx_DoMatch(Classes.Element parentElement, Classes.Element currentElement)
        {
            var matchOutput = new Classes.Output();
            var logicArr = currentElement.Logic.Split('|');

            matchOutput.OutputText = "false";

            if (logicArr.Length == 2)
            {
                var elemKey = logicArr[0].Trim();
                var field = logicArr[1].Trim();
                var newValue = MainClass.userInput.GetInputParams();

                var dbLogic = new DataManagement.GameLogic.Element();
                var success = dbLogic.SetElementField(elemKey, field, newValue);

                if (success)
                {
                    Message msg = new Message();
                    foreach (var elem in parentElement.Children)
                    {
                        if (elem.ElementType.Equals("message"))
                        {
                            matchOutput.OutputText = elem.Output.OutputFormat();
                        }
                    }
                }
            }

            return matchOutput;
        }
    }
}
