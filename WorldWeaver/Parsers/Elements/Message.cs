using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Message
    {
        public Classes.Output ParseMessage(Classes.Output output, string gameDb, Classes.Element msgElement)
        {
            if (msgElement.Output.Equals(""))
            {
                return output;
            }

            output.OutputText += Environment.NewLine + msgElement.Output;
            output.MatchMade = true;

            return output;
        }
    }
}
