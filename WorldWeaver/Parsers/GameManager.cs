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

                var procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.GameCache.Game.ElementType);
                foreach (var proc in procItems)
                {
                    foreach (var glob in Cache.GlobalCache.Global)
                    {
                        elemParser.ParseElement(glob, proc, false);
                    }
                }

                procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.PlayerCache.Player.ElementType);
                foreach (var proc in procItems)
                {
                    elemParser.ParseElement(Cache.PlayerCache.Player, proc);
                }

                procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);
                foreach (var proc in procItems)
                {
                    elemParser.ParseElement(Cache.RoomCache.Room, proc);
                }
            }
            else
            {
                if (!Cache.PlayerCache.Player.Name.Equals(""))
                {
                    if (!Cache.GameCache.GameInitialized)
                    {
                        CacheManager.RefreshCache();
                        var gameProcItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.GameCache.Game.ElementType);

                        foreach (var proc in gameProcItems)
                        {
                            elemParser.ParseElement(Cache.GameCache.Game, proc, false);
                        }

                        Cache.GameCache.GameInitialized = true;
                    }
                }

                // Parse the system events
                Tools.Game.IncrementTime();

                if (Cache.FightCache.Fight == null)
                {
                    var playerInv = elemDb.GetElementChildren(MainClass.gameDb, Cache.PlayerCache.Player.ElementKey);
                    foreach (var child in playerInv)
                    {
                        var playerProcItems = ProcFunctions.GetProcessStepsByType(child.ElementType);
                        foreach (var proc in playerProcItems)
                        {
                            elemParser.ParseElement(child, proc);
                        }
                    }

                    var locProcItems = ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);
                    foreach (var proc in locProcItems)
                    {
                        elemParser.ParseElement(Cache.RoomCache.Room, proc);
                    }
                }

                foreach (var glob in Cache.GlobalCache.Global)
                {
                    var globalProcItems = ProcFunctions.GetProcessStepsByType("global");
                    foreach (var proc in globalProcItems)
                    {
                        elemParser.ParseElement(glob, proc);
                    }
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
