using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public Classes.Output ParseMessage(Classes.Output output, string gameDb, Classes.Element msgElement)
        {
            output.OutputText += Environment.NewLine + msgElement.output;
            output.MatchMade = true;

            return output;
        }
    }
}
