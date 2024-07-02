using System;
using WorldWeaver.Tools;

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
                MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(Environment.NewLine + msgElement.Output, msgElement);
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
                        MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(Environment.NewLine + ProcessMessageText(msg.Output, msg.Tags), msg);
                        MainClass.output.MatchMade = true;
                        break;
                    }
                    else
                    {
                        idx++;
                    }
                }
            }

            var navElems = Cache.RoomCache.Room.Children.Where(c => c.ElementType.Equals("navigation"));
            if (MainClass.handledNavigation)
            {
                return;
            }

            foreach (var nav in navElems)
            {
                if (nav.Tags.TagsContain(parentElement.ElementType))
                {
                    MainClass.output.OutputText += Environment.NewLine + Environment.NewLine;
                    ParseMessage(Cache.RoomCache.Room, nav, false, 0);
                    MainClass.output.OutputText += Environment.NewLine + Environment.NewLine;
                    if (MainClass.output.MatchMade)
                    {
                        MainClass.handledNavigation = true;
                        return;
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
