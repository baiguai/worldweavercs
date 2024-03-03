using System;
using System.IO;
using System.Reflection;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GameParser
    {
        public string playerInput;
        public bool playingGame = false;
        public string gameDb = "";
        public string gameKey = "";

        public Classes.Output ParseInput(string input)
        {
            var output = new Classes.Output()
            { 
                MatchMade = false
            };
            var method = Tools.CommandFunctions.GetCommandMethod(input, "GameParser");

            playerInput = input;

            if (!method.Equals(""))
            {
                if (!output.MatchMade && method.Equals("DoPlayGame"))
                {
                    output = DoPlayGame(output);
                    var logic = new DataManagement.GameLogic.Element();
                    var player = logic.GetElementsByType(gameDb, "player");

                    if (output.MatchMade)
                    {
                        return output;
                    }

                    var mgr = new Parsers.GameManager();
                    output = mgr.ProcessGameInput(gameKey, gameDb, output, playerInput);
                    return output;
                }

                if (!output.MatchMade && method.Equals("DoResumeGame"))
                {
                    output = DoResumeGame(output);
                }

                if (!output.MatchMade && method.Equals("DoListGames"))
                {
                    output = DoListGames(output);
                }

                if (!output.MatchMade && method.Equals("DoSetPlayerName"))
                {
                    output = DoSetPlayerName(output);
                }
            }
            else
            {
                if (!gameKey.Equals(""))
                {
                    output = DoGameInput(output);
                }
            }

            return output;
        }

        private Classes.Output DoSetPlayerName(Output output)
        {
            var gameObj = new DataManagement.Game.PlayGame();
            Cache.GameCache.Game = null;

            output = gameObj.SetPlayerName(output, gameDb, playerInput.Replace("set player name ", "set_player_name ").GetInputParams());

            return output;
        }

        public Classes.Output DoPlayGame(Classes.Output output)
        {
            var gameFile = playerInput.Replace("play ", "");
            if (gameDb.Equals(""))
            {
                gameDb = $"{gameFile}_playing.db";
                File.Copy($"Games/{gameFile}.db", $"Games/{gameDb}", true);
            }

            output = InitiateGame(output);

            return output;
        }

        public Classes.Output DoResumeGame(Classes.Output output)
        {
            var gameFile = playerInput.Replace("resume ", "");
            if (gameDb.Equals(""))
            {
                gameDb = $"{gameFile}_playing.db";
            }

            DataManagement.GameLogic.Element elemLogic = new DataManagement.GameLogic.Element();
            DataManagement.GameLogic.Game gameLogic = new DataManagement.GameLogic.Game();
            var gameElem = elemLogic.GetElementsByType(gameDb, "game")[0];

            Cache.GameCache.Game = gameElem;

            if (gameElem != null)
            {
                playingGame = true;
            }

            foreach (var gameChild in gameElem.Children)
            {
                if (gameChild.ElementType.Equals("player"))
                {
                    Cache.PlayerCache.Player = gameChild;
                    break;
                }
            }

            output = InitiateGame(output);

            return output;
        }

        public Classes.Output InitiateGame(Classes.Output output)
        {
            var gameObj = new DataManagement.Game.PlayGame();
            var gameManager = new GameManager();

            output.MatchMade = false;

            output = gameObj.StartGame(output, gameDb);
            if (output.MatchMade)
            {
                output.OutputText = "The specified game key could not be found.";
                output.MatchMade = true;
                return output;
            }
            gameKey = output.Value;

            // Primary game processor
            if (!output.MatchMade)
            {
                var logic = new DataManagement.GameLogic.Element();
                var player = logic.GetElementsByType(gameDb, "player");

                if (player[0].Name.Equals(""))
                {
                    output.OutputText = $@"
To specify your player name use:
set player name <<NAME>>
                        ";
                    output.MatchMade = true;
                    return output;
                }

                output = gameManager.ProcessGameInput(gameKey, gameDb, output, playerInput);
            }

            // Final output
            if (output.MatchMade)
            {
                output.OutputText = output.OutputText.OutputFormat();
            }
            return output;
        }

        public Classes.Output DoGameInput(Classes.Output output)
        {
            if (!playingGame)
            {
                output.MatchMade = false;
                return output;
            }
            var gameManager = new GameManager();

            // Primary game processor
            if (!output.MatchMade)
            {
                output = gameManager.ProcessGameInput(gameKey, gameDb, output, playerInput);
            }

            // Final output
            if (output.MatchMade)
            {
                output.OutputText = output.OutputText.OutputFormat();
            }
            return output;
        }

        public Classes.Output DoListGames(Classes.Output output)
        {
            foreach (string file in Directory.GetFiles("Games"))
            {
                if (Path.GetExtension(file).Equals(".db") && !file.Contains("_playing"))
                {
                    if (!output.Equals(""))
                    {
                        output.OutputText += Environment.NewLine;
                    }

                    output.OutputText += Path.GetFileNameWithoutExtension(file);
                }
            }

            output.MatchMade = true;

            return output;
        }
    }
}
