using System;
using System.IO;
using System.Reflection;
using WorldWeaver.Classes;
using WorldWeaver.Tools;
using System.Globalization;

namespace WorldWeaver.Parsers
{
    public class GameParser
    {
        public bool playingGame = false;
        public string gameKey = "";
        public CommandHistoryManager cmdHist = new CommandHistoryManager();

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
                        var player = logic.GetElementsByType("player");

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
                        var gameFile = MainClass.userInput.ToLower().Replace("resume ", "").Replace(" ", "_");
                        if (!File.Exists($"Games/{gameFile}_playing.db"))
                        {
                            MainClass.userInput = $"play {gameFile}";
                            DoPlayGame();
                        }
                        else
                        {
                            DoResumeGame();
                        }
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoListGames"))
                    {
                        DoListGames();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoMenu"))
                    {
                        if (Cache.GameCache.Game == null)
                        {
                            Cache.GameCache.Game = new Element();
                        }
                        MainClass.output.OutputText = OutputProcessor.ProcessOutputText(Tools.InitFunctions.GetInitMessage(false), Cache.GameCache.Game);
                        MainClass.output.MatchMade = true;
                    }
                }

                if (DataManagement.GameLogic.Game.IsGameRunning() && duringGame)
                {
                    if (!MainClass.output.MatchMade && method.Equals("DoSetPlayerName"))
                    {
                        DoSetPlayerName();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoNoteAdd"))
                    {
                        DoAddNote();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoNoteDelete"))
                    {
                        DoDeleteNote();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoNotesList"))
                    {
                        DoListNotes();
                    }

                    if (!MainClass.output.MatchMade && method.Equals("DoNoteView"))
                    {
                        DoViewNote();
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
                    MainClass.userInput = cmdHist.HandleInput(MainClass.userInput);
                    DoGameInput();
                }
            }

            return;
        }

        private void DoTime()
        {
            DataManagement.GameLogic.Game gameData = new DataManagement.GameLogic.Game();

            MainClass.output.OutputText = gameData.GetTime();
            MainClass.output.MatchMade = true;
        }

        private void DoSetPlayerName()
        {
            var gameObj = new DataManagement.Game.PlayGame();
            Cache.GameCache.Game = null;

            gameObj.SetPlayerName(MainClass.userInput.Replace("set player name ", "set_player_name ", StringComparison.OrdinalIgnoreCase).GetInputParams());

            DoResumeGame(true);
        }

        private void DoAddNote()
        {
            var noteInput = MainClass.userInput.Replace("noteadd ", "", StringComparison.OrdinalIgnoreCase);
            var arr = noteInput.Split('|');
            if (arr.Length != 2)
            {
                MainClass.output.OutputText = $"To add a note use:{Environment.NewLine}noteadd <note key>|<note text>";
                return;
            }
            var noteKey = arr[0].Trim();
            var noteText = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            elemDb.AddNote(noteKey, noteText);
        }

        private void DoDeleteNote()
        {
            var noteInput = MainClass.userInput.Replace("notedelete ", "", StringComparison.OrdinalIgnoreCase);

            var elemDb = new DataManagement.GameLogic.Element();

            elemDb.DeleteNote(noteInput);
        }

        private void DoListNotes()
        {
            var elemDb = new DataManagement.GameLogic.Element();

            var notes = elemDb.ListNotes();

            MainClass.output.OutputText = $"Notes:";
            if (notes.Count > 0)
            {
                foreach (var nt in notes)
                {
                    MainClass.output.OutputText += $"{Environment.NewLine}{nt}";
                }
            }
            else
            {
                MainClass.output.OutputText += $"{Environment.NewLine}There are no saved notes";
            }
            MainClass.output.MatchMade = true;
        }

        private void DoViewNote()
        {
            var noteInput = MainClass.userInput.Replace("note ", "", StringComparison.OrdinalIgnoreCase);

            var elemDb = new DataManagement.GameLogic.Element();

            var noteTxt = elemDb.ViewNote(noteInput);

            if (!noteTxt.Equals(""))
            {
                MainClass.output.OutputText = noteTxt;
                MainClass.output.MatchMade = true;
            }
        }

        public void DoPlayGame()
        {
            if (Cache.GameCache.Game == null)
            {
                MainClass.gameDb = "";
            }

            var gameFile = MainClass.userInput.ToLower().Replace("play ", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "_");
            if (MainClass.gameDb.Equals(""))
            {
                MainClass.gameDb = $"{gameFile}_playing";

                try
                {
                    File.Copy($"Games/{gameFile}.db", $"Games/{MainClass.gameDb}.db", true);
                }
                catch (Exception)
                {
                    MainClass.output.OutputText = "Game file not found. Game names are case sensitive, so be sure it matches the game's case.";
                    MainClass.output.MatchMade = true;
                    MainClass.output.Error = true;
                    return;
                }
            }

            InitiateGame();
        }

        public void DoResumeGame(bool startingGame = false)
        {
            var gameFile = MainClass.userInput.ToLower().Replace("resume ", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "_");

            if (MainClass.gameDb.Equals(""))
            {
                MainClass.gameDb = $"{gameFile}_playing";
            }

            DataManagement.GameLogic.Element elemLogic = new DataManagement.GameLogic.Element();
            DataManagement.GameLogic.Game gameLogic = new DataManagement.GameLogic.Game();

            var gameElem = new Classes.Element();

            try
            {
                gameElem = elemLogic.GetElementsByType("game")[0];
            }
            catch (Exception)
            {
                MainClass.output.OutputText = "The specified game could not be found. Remember game names are case sensitive.";
                MainClass.output.MatchMade = true;
                return;
            }

            var playerElem = elemLogic.GetElementsByType("player")[0];
            var roomElem = elemLogic.GetElementByKey(playerElem.ParentKey);
            var elemParser = new Parsers.Elements.Element();

            Cache.GameCache.Game = gameElem;
            Cache.PlayerCache.Player = playerElem;
            Cache.RoomCache.Room = roomElem;

            // if (startingGame)
            // {
            Cache.GameCache.Game.ParseElement(true);
            // }

            Cache.RoomCache.Room.ParseElement(true);
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

            //

            // Primary game processor
            if (!MainClass.output.MatchMade)
            {
                var logic = new DataManagement.GameLogic.Element();
                var player = logic.GetElementsByType("player");

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
            var histComm = "";




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

                if (Cache.FightCache.Fight != null && !Cache.FightCache.Fight.PlayerHasAttacked && !Cache.FightCache.Fight.AllEnemiesDead)
                {
                    var atkParse = new Parsers.Elements.Attack();
                    Cache.FightCache.Fight.PlayersTurn = false;
                    atkParse.ProcessFightRound();
                }
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

                    var gameName = Path.GetFileNameWithoutExtension(file).Replace("_", " ");
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    gameName = textInfo.ToTitleCase(gameName);

                    MainClass.output.OutputText += gameName;
                }
            }

            MainClass.output.MatchMade = true;

            return;
        }
    }
}
