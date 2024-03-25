using System;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GameManager
    {
        public Classes.Output ProcessGameInput(string gameKey, string gameDb, Classes.Output output, string userInput)
        {
            var gameLogic = new DataManagement.GameLogic.Game();
            var elemParser = new Elements.Element();
            var logic = new DataManagement.GameLogic.Element();

            if (!DataManagement.GameLogic.Game.IsGameRunning())
            {
                if (Cache.GameCache.Game == null)
                {
                    CacheManager.RefreshCache(gameDb);
                }

                var procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.GameCache.Game.ElementType);
                foreach (var proc in procItems)
                {
                    foreach (var glob in Cache.GlobalCache.Global)
                    {
                        output = elemParser.ParseElement(output, gameDb, glob, userInput, proc, false);
                    }
                }

                procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.PlayerCache.Player.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, Cache.PlayerCache.Player, userInput, proc);
                }

                procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, Cache.RoomCache.Room, userInput, proc);
                }
            }
            else
            {
                if (!Cache.PlayerCache.Player.Name.Equals(""))
                {
                    if (!Cache.GameCache.GameInitialized)
                    {
                        CacheManager.RefreshCache(gameDb);
                        var gameProcItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.GameCache.Game.ElementType);

                        foreach (var proc in gameProcItems)
                        {
                            output = elemParser.ParseElement(output, gameDb, Cache.GameCache.Game, userInput, proc, false);
                        }

                        Cache.GameCache.GameInitialized = true;
                    }
                }

                var locProcItems = ProcFunctions.GetProcessStepsByType(Cache.RoomCache.Room.ElementType);
                foreach (var proc in locProcItems)
                {
                    output = elemParser.ParseElement(output, gameDb, Cache.RoomCache.Room, userInput, proc);
                }

                foreach (var glob in Cache.GlobalCache.Global)
                {
                    var globalProcItems = ProcFunctions.GetProcessStepsByType("global");
                    foreach (var proc in globalProcItems)
                    {
                        output = elemParser.ParseElement(output, gameDb, glob, userInput, proc);
                    }
                }
            }
                
            return output;
        }
    }
}
