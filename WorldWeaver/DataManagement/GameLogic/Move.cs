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

            newParentKey = HandleSpecialLocations(newParentKey);

            foreach (var tag in tagList)
            {
                var key = tag;

                if (key.Equals("[player]"))
                {
                    key = Cache.PlayerCache.Player.ElementKey;
                }

                success = elemDb.SetElementParentKey(key, newParentKey);

                if (success)
                {
                    Tools.CacheManager.RefreshCache();
                    MainClass.output.OutputText += outputText;
                    Cache.RoomCache.Room.ParseElement(true);
                }

                if (key.Equals(Cache.PlayerCache.Player.ElementKey))
                {
                    var trvParser = new Parsers.Elements.Travel();
                    trvParser.ParseTravel();
                }
            }

            return;
        }

        private string HandleSpecialLocations(string newParentKey)
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
