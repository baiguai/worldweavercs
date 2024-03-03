using System;
using System.Reflection;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class AdminParser
    {
        public string playerInput = "";

        public Classes.Output ParseInput(string input)
        {
            var output = new Classes.Output()
            { 
                MatchMade = false
            };
            var method = Tools.CommandFunctions.GetCommandMethod(input, "AdminParser");

            if (!method.Equals(""))
            {
                playerInput = input;

                if (!output.MatchMade && method.Equals("DoBuildGame"))
                {
                    DoBuildGame(output);
                }

                if (!output.MatchMade && method.Equals("DoAdminHelp"))
                {
                    DoAdminHelp(output);
                }
            }

            return output;
        }


        public void DoBuildGame(Classes.Output output)
        {
            output.OutputText = "Could not build the game database.";
            DataManagement.Game.BuildGame builder = new DataManagement.Game.BuildGame();

            var success = LoadGameData(playerInput.GetInputParamSingle());

            if (success)
            {
                output.MatchMade = true;
                output.OutputText = "Game database successfully built.";
            }
        }

        private bool LoadGameData(string gameKey)
        {
            var success = false;
            DataManagement.Game.BuildGame builder = new DataManagement.Game.BuildGame();

            success = builder.LoadGame(gameKey);

            return success;
        }

        public void DoAdminHelp(Classes.Output output)
        {
            var parms = playerInput.GetInputParams();

            if (parms.Equals(""))
            {
                parms = "_help";
            }

            output.MatchMade = true;
            output.OutputText = Tools.CommandFunctions.GetHelpTopic(parms, "Admin");
        }
    }
}
