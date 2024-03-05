using System;

namespace WorldWeaver.Parsers
{
    public class GameManager
    {
        public Classes.Output ProcessGameInput(string gameKey, string gameDb, Classes.Output output, string userInput)
        {
            var gameLogic = new DataManagement.GameLogic.Game();
            var elemLogic = new DataManagement.GameLogic.Element();
            var elemParser = new Elements.Element();
            var logic = new DataManagement.GameLogic.Element();

            if (!gameLogic.IsGameRunning(gameDb))
            {
                var gameElem = elemLogic.GetElementsByType(gameDb, "game")[0];
                Cache.GameCache.Game = gameElem;

                foreach (var gameChild in gameElem.Children)
                { 
                    if (gameChild.ElementType.Equals("player"))
                    {
                        Cache.PlayerCache.Player = gameChild;
                        break;
                    }
                }

                var procItems = Tools.ProcFunctions.GetProcessStepsByType(gameElem.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, gameElem, userInput, proc);
                }

                procItems = Tools.ProcFunctions.GetProcessStepsByType(Cache.PlayerCache.Player.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, Cache.PlayerCache.Player, userInput, proc);
                }

                var locElem = elemLogic.GetElementByKey(gameDb, Cache.PlayerCache.Player.Location);
                procItems = Tools.ProcFunctions.GetProcessStepsByType(locElem.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, locElem, userInput, proc);
                }
            }
            else
            {
                if (!Cache.PlayerCache.Player.Name.Equals(""))
                {
                    if (!Cache.GameCache.GameInitialized)
                    {
                        var gameElem = logic.GetElementByKey(gameDb, gameKey);
                        var gameProcItems = Tools.ProcFunctions.GetProcessStepsByType(gameElem.ElementType);

                        foreach (var proc in gameProcItems)
                        {
                            output = elemParser.ParseElement(output, gameDb, gameElem, userInput, proc);
                        }

                        Cache.GameCache.GameInitialized = true;
                    }
                }

                var player = logic.GetElementByKey(gameDb, "player");
                var locElem = logic.GetElementByKey(gameDb, player.Location);
                var locProcItems = Tools.ProcFunctions.GetProcessStepsByType(locElem.ElementType);

                foreach (var proc in locProcItems)
                {
                    output = elemParser.ParseElement(output, gameDb, locElem, userInput, proc);
                }
            }
                
            return output;
        }
    }
}
