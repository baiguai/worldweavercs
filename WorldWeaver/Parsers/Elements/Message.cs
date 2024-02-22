using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public Classes.Output ParseMessage(string gameDb, Classes.Element msgElement)
        {
            var output = new Classes.Output();

            output.OutputText = msgElement.output;
            output.MatchMade = true;

            return output;
        }
    }
}
