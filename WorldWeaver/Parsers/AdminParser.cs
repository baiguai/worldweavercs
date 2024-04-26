using System;
using System.Reflection;
using WorldWeaver.Tools;

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
                if (!MainClass.output.MatchMade && method.Equals("DoBuildGame"))
                {
                    DoBuildGame();
                }

                if (!MainClass.output.MatchMade && method.Equals("DoAdminHelp"))
                {
                    DoAdminHelp();
                }
            }
        }


        public void DoBuildGame()
        {
            MainClass.output.OutputText = "Could not build the game database.";
            DataManagement.Game.BuildGame builder = new DataManagement.Game.BuildGame();

            var success = LoadGameData(MainClass.userInput.GetInputParamSingle());

            if (success)
            {
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = "Game database successfully built.";
            }
        }

        private bool LoadGameData(string gameKey)
        {
            var success = false;
            DataManagement.Game.BuildGame builder = new DataManagement.Game.BuildGame();

            success = builder.LoadGame(gameKey);

            return success;
        }

        public void DoAdminHelp()
        {
            var parms = MainClass.userInput.GetInputParams();

            if (parms.Equals(""))
            {
                parms = "_help";
            }

            MainClass.output.MatchMade = true;
            MainClass.output.OutputText = Tools.CommandFunctions.GetHelpTopic(parms, "Admin");
        }
    }
}
