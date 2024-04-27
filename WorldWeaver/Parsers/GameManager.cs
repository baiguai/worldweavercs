using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GameManager
    {
        public void ProcessGameInput()
        {
            var gameLogic = new DataManagement.GameLogic.Game();
            var elemParser = new Elements.Element();
            var logic = new DataManagement.GameLogic.Element();
            var elemDb = new DataManagement.GameLogic.Element();

            if (!DataManagement.GameLogic.Game.IsGameRunning())
            {
                if (Cache.GameCache.Game == null)
                {
                    CacheManager.RefreshCache();
                }

                foreach (var glob in Cache.GlobalCache.Global)
                {
                    glob.ParseElement();
                }
                Cache.PlayerCache.Player.ParseElement();
                Cache.RoomCache.Room.ParseElement();
            }
            else
            {
                if (!Cache.PlayerCache.Player.Name.Equals(""))
                {
                    if (!Cache.GameCache.GameInitialized)
                    {
                        CacheManager.RefreshCache();
                        Cache.GameCache.Game.ParseElement();
                        Cache.GameCache.GameInitialized = true;
                    }
                }

                // Parse the system events
                Tools.Game.IncrementTime();

                if (Cache.FightCache.Fight == null)
                {
                    var playerInv = elemDb.GetElementChildren(Cache.PlayerCache.Player.ElementKey);
                    foreach (var child in playerInv)
                    {
                        child.ParseElement();
                    }
                    Cache.RoomCache.Room.ParseElement();
                }

                foreach (var glob in Cache.GlobalCache.Global)
                {
                    glob.ParseElement();
                }

                if (Cache.GameCache.Game == null)
                {
                    return;
                }

                var trvParser = new Parsers.Elements.Travel();
                trvParser.ParseTravel();
            }
        }
    }
}
