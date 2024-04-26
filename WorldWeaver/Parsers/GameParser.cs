using System;
using System.IO;
using System.Reflection;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GameParser
    {
        public bool playingGame = false;
        public string gameKey = "";

        public void ParseInput()
        {
            MainClass.output.MatchMade = false;
            MainClass.output.OutputText = "";

            var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "GameParser");
            var duringGame = Tools.CommandFunctions.GetDuringGameOption(MainClass.userInput, "GameParser");

            if (!method.Equals(""))
            {
                if (!DataManagement.GameLogic.Game.IsGameRunning() && !duringGame)
                {
                    if (!MainClass.output.MatchMade && method.Equals("DoPlayGame"))
                    {
                        DoPlayGame();
                        if (MainClass.output.Error)
                        {
                            return;
                        }
                        var logic = new DataManagement.GameLogic.Element();
                        var player = logic.GetElementsByType(MainClass.gameDb, "player");

                        if (MainClass.output.MatchMade)
                        {
                            return;
                        }

                        var mgr = new Parsers.GameManager();
                        mgr.ProcessGameInput();
                        return;
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoResumeGame"))
                    {
                        DoResumeGame();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoListGames"))
                    {
                        DoListGames();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoSetPlayerName"))
                    {
                        DoSetPlayerName();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoMenu"))
                    {
                        MainClass.output.OutputText = Tools.InitFunctions.GetInitMessage(false);
                        MainClass.output.MatchMade = true;
                    }
                }
                if (DataManagement.GameLogic.Game.IsGameRunning() && duringGame)
                {
                    if (!MainClass.output.MatchMade && method.Equals("DoQuit"))
                    {
                        DoQuit();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoTime"))
                    {
                        DoTime();
                    }
                }
            }
            else
            {
                if (DataManagement.GameLogic.Game.IsGameRunning())
                {
                    DoGameInput();
                }
            }

            return;
        }

        private void DoTime()
        {
            DataManagement.GameLogic.Game gameData = new DataManagement.GameLogic.Game();

            MainClass.output.OutputText = gameData.GetTime(MainClass.gameDb);
            MainClass.output.MatchMade = true;
        }

        private void DoQuit()
        {
            Tools.CacheManager.ClearCache();

            MainClass.output.OutputText = Tools.InitFunctions.GetInitMessage(false);
            MainClass.output.MatchMade = true;
        }

        private void DoSetPlayerName()
        {
            var gameObj = new DataManagement.Game.PlayGame();
            Cache.GameCache.Game = null;

            gameObj.SetPlayerName(MainClass.userInput.Replace("set player name ", "set_player_name ").GetInputParams());

            DoResumeGame(true);
        }

        public void DoPlayGame()
        {
            if (Cache.GameCache.Game == null)
            {
                MainClass.gameDb = "";
            }

            var gameFile = MainClass.userInput.Replace("play ", "");
            if (MainClass.gameDb.Equals(""))
            {
                MainClass.gameDb = $"{gameFile}_playing.db";

                try
                {
                    File.Copy($"Games/{gameFile}.db", $"Games/{MainClass.gameDb}", true);
                }
                catch (Exception)
                {
                    MainClass.output.OutputText = "Game file not found. Game names are case sensitive, so be sure it matches the game's case.";
                    MainClass.output.MatchMade = true;
                    MainClass.output.Error = true;
                }
            }

            InitiateGame();
        }

        public void DoResumeGame(bool startingGame = false)
        {
            var gameFile = MainClass.userInput.Replace("resume ", "");
            if (MainClass.gameDb.Equals(""))
            {
                MainClass.gameDb = $"{gameFile}_playing.db";
            }

            DataManagement.GameLogic.Element elemLogic = new DataManagement.GameLogic.Element();
            DataManagement.GameLogic.Game gameLogic = new DataManagement.GameLogic.Game();

            var gameElem = new Classes.Element();

            try
            {
                gameElem = elemLogic.GetElementsByType(MainClass.gameDb, "game")[0];
            }
            catch (Exception)
            {
                MainClass.output.OutputText = "The specified game could not be found. Remember game names are case sensitive.";
                MainClass.output.MatchMade = true;
                return;
            }

            var playerElem = elemLogic.GetElementsByType(MainClass.gameDb, "player")[0];
            var roomElem = elemLogic.GetElementByKey(MainClass.gameDb, playerElem.ParentKey);
            var elemParser = new Parsers.Elements.Element();

            Cache.GameCache.Game = gameElem;
            Cache.PlayerCache.Player = playerElem;
            Cache.RoomCache.Room = roomElem;

            if (startingGame)
            {
                var gameProcs = ProcFunctions.GetProcessStepsByType("game");
                foreach (var proc in gameProcs)
                {
                    elemParser.ParseElement(Cache.GameCache.Game, proc, true);
                }
            }

            var procItems = ProcFunctions.GetProcessStepsByType("room");
            foreach (var proc in procItems)
            {
                elemParser.ParseElement(Cache.RoomCache.Room, proc, true);
            }
        }

        public void InitiateGame()
        {
            var gameObj = new DataManagement.Game.PlayGame();
            var gameManager = new GameManager();

            MainClass.output.MatchMade = false;

            gameObj.StartGame();
            if (MainClass.output.MatchMade)
            {
                MainClass.output.OutputText = "The specified game key could not be found.";
                MainClass.output.MatchMade = true;
                return;
            }
            gameKey = MainClass.output.Value;

            // Primary game processor
            if (!MainClass.output.MatchMade)
            {
                var logic = new DataManagement.GameLogic.Element();
                var player = logic.GetElementsByType(MainClass.gameDb, "player");

                if (player[0].Name.Equals(""))
                {
                    MainClass.output.OutputText = $@"
To specify your player name use:
set player name <<NAME>>
                        ";
                    MainClass.output.MatchMade = true;
                    return;
                }

                gameManager.ProcessGameInput();
            }

            // Final output
            if (MainClass.output.MatchMade)
            {
                MainClass.output.OutputText = MainClass.output.OutputText.OutputFormat();
            }
            return;
        }

        public void DoGameInput()
        {
            if (!DataManagement.GameLogic.Game.IsGameRunning())
            {
                MainClass.output.MatchMade = false;
                return;
            }
            var gameManager = new GameManager();

            // Primary game processor
            if (!MainClass.output.MatchMade)
            {
                gameManager.ProcessGameInput();
            }

            // Final output
            if (MainClass.output.MatchMade)
            {
                MainClass.output.OutputText = MainClass.output.OutputText.OutputFormat();
            }
            return;
        }

        public void DoListGames()
        {
            foreach (string file in Directory.GetFiles("Games"))
            {
                if (Path.GetExtension(file).Equals(".db") && !file.Contains("_playing"))
                {
                    if (!MainClass.output.Equals(""))
                    {
                        MainClass.output.OutputText += Environment.NewLine;
                    }

                    MainClass.output.OutputText += Path.GetFileNameWithoutExtension(file);
                }
            }

            MainClass.output.MatchMade = true;

            return;
        }
    }
}
