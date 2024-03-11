using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public Classes.Output ParseMessage(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element msgElement, bool allowRepeatOptions, int currentIndex)
        {
            if (msgElement.Output.Equals(""))
            {
                return output;
            }

            if (!allowRepeatOptions)
            {
                output.OutputText += Environment.NewLine + msgElement.Output;
                output.MatchMade = true;
            }
            else
            {
                var msgs = Tools.Elements.GetElementsByType(parentElement, "message");
                var idx = 0;
                foreach (var msg in msgs)
                {
                    if (idx == currentIndex)
                    {
                        output.OutputText += Environment.NewLine + msg.Output;
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
    }
}
