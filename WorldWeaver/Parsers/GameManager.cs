using System;
using WorldWeaver.Classes;
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
            Cache.RoomCache.Room.ParseElement(true);

            DoNavigation();
        }

        private void ParseRunningGame()
        {
            var gameLogic = new DataManagement.GameLogic.Game();
            var helpParser = new HelpParser();
            var devParser = new DevToolsParser();
            var elemParser = new Elements.Element();
            var logic = new DataManagement.GameLogic.Element();
            var elemDb = new DataManagement.GameLogic.Element();
            var histCount = Convert.ToInt32(AppSettingFunctions.GetConfigValue("history", "count"));

            Tools.History.AddHistoryItem(MainClass.userInput, histCount);

            if (!Cache.PlayerCache.Player.Name.Equals("") && !Cache.GameCache.GameInitialized)
            {
                CacheManager.RefreshCache();
                Cache.GameCache.Game.ParseElement();
                Cache.GameCache.GameInitialized = true;
            }

            helpParser.ParseInput();
            if (MainClass.output.MatchMade)
            {
                return;
            }

            MainClass.output.FailedLogic = false;
            devParser.ParseInput();
            if (MainClass.output.MatchMade)
            {
                return;
            }

            // Parse the system events
            Tools.Game.IncrementTime();

            var roundElems = elemDb.GetElementsByType("round");
            foreach (var rnd in roundElems)
            {
                rnd.ParseElement();
            }


            if (Cache.FightCache.Fight != null &&
                !MainClass.macro.IsRunning)
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
                else
                {
                    if (Cache.FightCache.Fight.AllEnemiesDead)
                    {
                        Cache.FightCache.Fight = null;
                    }
                    else
                    {
                        var attackables = elemDb.GetRoomElementsByTag("attackable", Cache.RoomCache.Room.ElementKey);
                        foreach (var attackable in attackables)
                        {
                            attackable.ParseElement(false);
                        }
                    }
                }
            }

            foreach (var glob in Cache.GlobalCache.Global)
            {
                MainClass.output.FailedLogic = false;
                glob.ParseElement();
            }

            var playerInv = elemDb.GetElementChildren(Cache.PlayerCache.Player.ElementKey, false);
            foreach (var child in playerInv)
            {
                MainClass.output.FailedLogic = false;
                child.ParseElement();
            }

            Cache.RoomCache.Room.ParseElement();

            var trvParser = new Parsers.Elements.Travel();
            trvParser.ParseTravel();

            if (Cache.GameCache.Game == null)
            {
                return;
            }

            if (MainClass.adminEnabled)
            {
                MainClass.output.FailedLogic = false;
                Cache.PlayerCache.Player.ParseElement();
            }

            // Navigation
            var parent = new Classes.Element();
            DoNavigation();
        }

        public void DoNavigation()
        {
            var elemParser = new Elements.Element();

            foreach (var nav in Cache.RoomCache.Room.Children.Where(r => r.ElementType.Equals("navigation", StringComparison.OrdinalIgnoreCase)))
            {
                var procs = Cache.RoomCache.Room.GetProcs();

                MainClass.output.OutputText = MainClass.output.OutputText.TrimEndBreaks();

                foreach (var proc in procs)
                {
                    elemParser.ParseElement(nav, proc, false);
                    if (MainClass.output.MatchMade)
                    {
                        break;
                    }
                }
            }
        }
    }
}
