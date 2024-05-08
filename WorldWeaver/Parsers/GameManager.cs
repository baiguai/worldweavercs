using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GameManager
    {
        public void ProcessGameInput()
        {
            if (!DataManagement.GameLogic.Game.IsGameRunning())
            {
                CreateNewGameCache();
            }
            else
            {
                ParseRunningGame();
            }
        }

        private void CreateNewGameCache()
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

        private void ParseRunningGame()
        {
            var gameLogic = new DataManagement.GameLogic.Game();
            var elemParser = new Elements.Element();
            var logic = new DataManagement.GameLogic.Element();
            var elemDb = new DataManagement.GameLogic.Element();

            if (!Cache.PlayerCache.Player.Name.Equals("") && !Cache.GameCache.GameInitialized)
            {
                CacheManager.RefreshCache();
                Cache.GameCache.Game.ParseElement();
                Cache.GameCache.GameInitialized = true;
            }

            // Parse the system events
            Tools.Game.IncrementTime();

            var roundElems = elemDb.GetElementsByType("round");
            foreach (var rnd in roundElems)
            {
                rnd.ParseElement();
            }


            if (Cache.FightCache.Fight != null)
            {
                var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "FightParser");

                if (method.Equals("DoFlee"))
                {
                    Cache.FightCache.Fight.PlayersTurn = false;
                    Cache.FightCache.Fight.PlayerFleeing = true;

                    var atkPars = new Parsers.Elements.Attack();
                    atkPars.ProcessFightRound();
                    return;
                }
            }


            var playerInv = elemDb.GetElementChildren(Cache.PlayerCache.Player.ElementKey);
            foreach (var child in playerInv)
            {
                child.ParseElement();
            }
            Cache.RoomCache.Room.ParseElement();

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
