using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Move
    {
        internal Output MoveElement(Output output, string gameDb, string outputText, string subject, string newParentKey, string userInput)
        {
            var tagList = subject.Split('|');
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = new Parsers.Elements.Element();
            var success = false;

            foreach (var tag in tagList)
            {
                success = elemDb.SetElementParentKey(gameDb, tag, newParentKey);
            }

            if (success)
            {
                var procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);

                output.OutputText += outputText;

                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, Cache.RoomCache.Room, userInput, proc, true);
                }
            }

            var test = Cache.RoomCache.Room;

            return output;
        }
    }
}
