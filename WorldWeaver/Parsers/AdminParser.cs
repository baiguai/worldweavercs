using System;
using System.Reflection;
using WorldWeaver.Tools;
using TextCopy;

namespace WorldWeaver.Parsers
{
    public class AdminParser
    {
        public void ParseInput()
        {
            MainClass.output.MatchMade = false;
            MainClass.output.OutputText = "";

            var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "AdminParser");

            if (!method.Equals(""))
            {
                if (!MainClass.output.MatchMade && method.Equals("DoValidate"))
                {
                    DoValidate();
                }

                if (!MainClass.output.MatchMade && method.Equals("DoBuildGame"))
                {
                    DoBuildGame();
                }

                if (!MainClass.output.MatchMade && method.Equals("DoValidateGame"))
                {
                    DoValidateGame();
                }

                if (!MainClass.output.MatchMade && method.Equals("DoAdminHelp"))
                {
                    DoAdminHelp();
                }

                if (!MainClass.output.MatchMade && method.Equals("DoSnip"))
                {
                    DoSnip();
                }
            }
        }

        private void DoValidate()
        {
            var validParse = new ValidationParser();
            validParse.ProcessGameValidation(MainClass.userInput.GetInputParamSingle());
        }

        public void DoBuildGame()
        {
            MainClass.output.ExitFlow = true;
            MainClass.output.OutputText = "Could not build the game database.";

            var success = LoadGameData(MainClass.userInput.GetInputParamSingle());

            if (success)
            {
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = "Game database successfully built.";
            }
        }

        public void DoValidateGame()
        {
            MainClass.output.OutputText = "Could not validate the game database.";

            ValidateGameData(MainClass.userInput.GetInputParamSingle());

            if (MainClass.output.OutputText.Equals(""))
            {
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = "Game validation was successful.";
            }
        }

        private bool LoadGameData(string gameKey)
        {
            var success = false;
            DataManagement.Game.BuildGame builder = new DataManagement.Game.BuildGame();

            success = builder.LoadGame(gameKey);

            return success;
        }

        private void ValidateGameData(string gameKey)
        {
            DataManagement.Game.ValidateGame validator = new DataManagement.Game.ValidateGame();

            validator.Validate(gameKey);
        }

        public void DoAdminHelp()
        {
            var parms = MainClass.userInput.GetInputParams();

            if (parms.Equals(""))
            {
                parms = "_help";
            }

            MainClass.output.MatchMade = true;
            MainClass.output.OutputText = Tools.CommandFunctions.GetHelpTopic(parms, "Help/Admin");
        }

        private void DoSnip()
        {
            var snip = MainClass.userInput.GetInputParamSingle();

            if (snip.Equals(""))
            {
                ListSnippets();
            }
            else
            {
                LoadSnippet(snip);
            }

            MainClass.output.MatchMade = true;
        }

        private void ListSnippets()
        {
            foreach (string file in Directory.GetFiles("Snippets"))
            {
                if (Path.GetExtension(file).Equals(".snp"))
                {
                    if (!MainClass.output.Equals(""))
                    {
                        if (MainClass.output.OutputText.Equals("\n"))
                        {
                            MainClass.output.OutputText = "";
                        }
                        MainClass.output.OutputText += Environment.NewLine;
                    }


                    if (MainClass.output.OutputText.Equals("\n"))
                    {
                        MainClass.output.OutputText = "";
                    }
                    MainClass.output.OutputText += Path.GetFileNameWithoutExtension(file);
                }
            }

            return;
        }

        private void LoadSnippet(string snip)
        {
            foreach (string file in Directory.GetFiles("Snippets"))
            {
                if (Path.GetExtension(file).Equals(".snp") && Path.GetFileNameWithoutExtension(file).Equals(snip))
                {
                    var contents = File.ReadAllLines(file);

                    foreach (var line in contents)
                    {
                        if (MainClass.output.OutputText.Equals("\n"))
                        {
                            MainClass.output.OutputText = "";
                        }
                        if (!MainClass.output.OutputText.Equals(""))
                        {
                            MainClass.output.OutputText += Environment.NewLine;
                        }
                            
                        MainClass.output.OutputText += line;
                    }
                }
            }
        }
    }
}
