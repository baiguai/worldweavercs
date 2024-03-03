using System;

namespace WorldWeaver.Parsers
{
    public class GameManager
    {
        public Classes.Output ProcessGameInput(string gameKey, string gameDb, Classes.Output output, string userInput)
        {
            DataManagement.GameLogic.Game gameLogic = new DataManagement.GameLogic.Game();
            DataManagement.GameLogic.Element elemLogic = new DataManagement.GameLogic.Element();
            Elements.Element elemParser = new Elements.Element();

            if (!gameLogic.IsGameRunning(gameDb))
            {
                var procTypes = Tools.ProcFunctions.GetProcessStepsByType("game");
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
                    var logic = new DataManagement.GameLogic.Element();
                    var player = logic.GetElementByKey(gameDb, "player");

                    if (player.Location.Equals(""))
                    {
                        var gameElem = logic.GetElementByKey(gameDb, gameKey);
                        var procItems = Tools.ProcFunctions.GetProcessStepsByType(gameElem.ElementType);

                        foreach (var proc in procItems)
                        {
                            output = elemParser.ParseElement(output, gameDb, gameElem, userInput, proc);
                        }
                    }
                    else
                    {
                        var locElem = logic.GetElementByKey(gameDb, player.Location);
                        var procItems = Tools.ProcFunctions.GetProcessStepsByType(locElem.ElementType);

                        foreach (var proc in procItems)
                        {
                            output = elemParser.ParseElement(output, gameDb, locElem, userInput, proc);
                        }
                    }
                }
            }
                
            return output;
        }
    }
}
