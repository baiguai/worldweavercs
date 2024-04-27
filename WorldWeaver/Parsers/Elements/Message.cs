using System;

namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public void ParseMessage(Classes.Element parentElement, Classes.Element msgElement, bool allowRepeatOptions, int currentIndex)
        {
            var elemParser = new Parsers.Elements.Element();

            if (msgElement.Output.Equals(""))
            {
                return;
            }

            if (!allowRepeatOptions)
            {
                MainClass.output.OutputText += Environment.NewLine + msgElement.Output;
                MainClass.output.MatchMade = true;

                msgElement.ParseElement();
            }
            else
            {
                var msgs = Tools.Elements.GetElementsByType(parentElement, "message");
                var idx = 0;
                foreach (var msg in msgs)
                {
                    if (idx == currentIndex)
                    {
                        MainClass.output.OutputText += Environment.NewLine + ProcessMessageText(msg.Output, msg.Tags);
                        MainClass.output.MatchMade = true;
                        break;
                    }
                    else
                    {
                        idx++;
                    }
                }
            }

            return;
        }

        public string ProcessMessageText(string outputText, string tag)
        {
            var messageOutput = outputText;

            if (!MainClass.userInput.Equals("") && !tag.Equals(""))
            {
                messageOutput = messageOutput.Replace("[input]", MainClass.userInput).Replace(tag, "").Trim();
            }

            return messageOutput;
        }
    }
}
