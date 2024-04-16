using System;

namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public Classes.Output ParseMessage(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element msgElement, string userInput, bool allowRepeatOptions, int currentIndex)
        {
            var elemParser = new Parsers.Elements.Element();

            if (msgElement.Output.Equals(""))
            {
                return output;
            }

            if (!allowRepeatOptions)
            {
                output.OutputText += Environment.NewLine + msgElement.Output;
                output.MatchMade = true;

                var msgProcs = Tools.ProcFunctions.GetProcessStepsByType(msgElement.ElementType);
                foreach (var proc in msgProcs)
                {
                    output = elemParser.ParseElement(output, gameDb, msgElement, "", proc, false);
                }
            }
            else
            {
                var msgs = Tools.Elements.GetElementsByType(parentElement, "message");
                var idx = 0;
                foreach (var msg in msgs)
                {
                    if (idx == currentIndex)
                    {
                        output.OutputText += Environment.NewLine + ProcessMessageText(msg.Output, userInput, msg.Tags); //@todo
                        output.MatchMade = true;
                        break;
                    }
                    else
                    {
                        idx++;
                    }
                }
            }

            return output;
        }

        public string ProcessMessageText(string outputText, string userInput, string tag)
        {
            var output = "";

            output = outputText.Replace("[input]", userInput).Replace(tag, "").Trim();

            return output;
        }
    }
}
