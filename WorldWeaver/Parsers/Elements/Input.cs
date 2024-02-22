using System;
using System.Text.RegularExpressions;

namespace WorldWeaver.Parsers.Elements
{
    public class Input
    {
        public Classes.Output ParseInput(string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            var output = new Classes.Output();
            output.MatchMade = false;

            Regex rgx = new Regex(currentElement.syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(userInput))
            {
                switch (parentElement.element_type)
                {
                    case "set_field":
                        var set_field = new SetField();
                        output = set_field.DoMatch(gameDb, parentElement, currentElement, userInput);
                        output.MatchMade = true;
                        break;
                }
            }
            else
            { 
                if (Helpers.HasEnterMessage(currentElement))
                {
                    var msgElem = Helpers.GetChildByType(currentElement, "enter_message");
                    var msg = new Message();

                    output = msg.ParseMessage(gameDb, msgElem);
                    output.MatchMade = true;
                }
            }

            return output;
        }
    }
}
