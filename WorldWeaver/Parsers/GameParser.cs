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
                    if (!output.MatchMade) // In this case a false means we can move on
                    {
                        output.OutputText = $@"
To specify your player name use:
set player name <<NAME>>
                        ";
                        output.MatchMade = true;
                        return output;
                    }
                }

                if (!output.MatchMade && method.Equals("DoListGames"))
                {
                    DoListGames(output);
                }

                if (!output.MatchMade && method.Equals("DoSetPlayerName"))
                {
                    DoSetPlayerName(output);
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

            output = gameObj.SetPlayerName(output, gameDb, playerInput.Replace("set player name ", "set_player_name ").GetInputParams());

            return output;
        }

        public Classes.Output DoPlayGame(Classes.Output output)
        {
            if (gameDb.Equals(""))
            {
                gameDb = $"{playerInput.Replace("play ", "").Trim()}.db";
            }
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
            gameKey = output.OutputText;

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

        public void DoListGames(Classes.Output output)
        {
            foreach (string file in Directory.GetFiles("Games"))
            {
                if (Path.GetExtension(file).Equals(".db"))
                {
                    if (!output.Equals(""))
                    {
                        output.OutputText += Environment.NewLine;
                    }

                    output.OutputText += Path.GetFileNameWithoutExtension(file);
                }
            }

            output.MatchMade = true;
        }
    }
}
