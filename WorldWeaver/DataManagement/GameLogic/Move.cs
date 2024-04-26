using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Move
    {
        internal void MoveElement(string outputText, string subject, string newParentKey)
        {
            var tagList = subject.Split('|');
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = new Parsers.Elements.Element();
            var success = false;

            newParentKey = HandleSpecialLocations(gameDb, newParentKey);

            foreach (var tag in tagList)
            {
                success = elemDb.SetElementParentKey(gameDb, tag, newParentKey);

                if (tag.Equals(Cache.PlayerCache.Player.ElementKey))
                {
                    var trvParser = new Parsers.Elements.Travel();
                    trvParser.ParseTravel(gameDb);
                }
            }

            if (success)
            {
                Tools.CacheManager.RefreshCache(gameDb);

                var procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);

                output.OutputText += outputText;

                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, Cache.RoomCache.Room, userInput, proc, true);
                }
            }

            return output;
        }

        private string HandleSpecialLocations(string gameDb, string newParentKey)
        {
            switch (newParentKey)
            {
                case "[room]":
                    return Cache.RoomCache.Room.ElementKey;
            }

            return newParentKey;
        }
    }
}
