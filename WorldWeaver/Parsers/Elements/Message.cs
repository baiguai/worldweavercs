﻿using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public void ParseMessage(Classes.Element parentElement, Classes.Element msgElement, bool allowRepeatOptions, int currentIndex)
        {
            var elemParser = new Parsers.Elements.Element();
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }

            if (msgElement.Output.Equals(""))
            {
                return;
            }

            if (parentElement.ElementType.Equals("navigation"))
            {
                MainClass.output.OutputText += Environment.NewLine;
            }

            if (!allowRepeatOptions)
            {
                MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(Environment.NewLine + msgElement.Output, msgElement);
                MainClass.output.MatchMade = true;

                if (parentElement.ElementType.Equals("navigation"))
                {
                    MainClass.output.OutputText += Environment.NewLine;
                }

                msgElement.ParseElement();
            }
            else
            {
                var msgs = Tools.Elements.GetElementsByType(parentElement, "message", true);
                var idx = 0;
                foreach (var msg in msgs)
                {
                    if (idx == currentIndex)
                    {
                        MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(Environment.NewLine + ProcessMessageText(msg.Output, msg.Tags), msg);
                        MainClass.output.MatchMade = true;

                        if (parentElement.ElementType.Equals("navigation"))
                        {
                            MainClass.output.OutputText += Environment.NewLine;
                        }
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
                    ParseMessage(Cache.RoomCache.Room, nav, false, 0);
                    if (MainClass.output.MatchMade)
                    {
                        MainClass.output.OutputText = Environment.NewLine + Environment.NewLine + MainClass.output.OutputText;
                        MainClass.output.OutputText += Environment.NewLine + Environment.NewLine;
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
