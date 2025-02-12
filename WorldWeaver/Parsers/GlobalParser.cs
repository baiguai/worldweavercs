using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class GlobalParser
    {
        public string playerInput = "";

        public void ParseInput()
        {
            MainClass.output.MatchMade = false;
            MainClass.output.OutputText = "";
            var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "GlobalParser");

            if (!method.Equals(""))
            {
                playerInput = MainClass.userInput;

                switch (method)
                {
                    case "DoExit":
                        MainClass.output.MatchMade = true;
                        MainClass.output.OutputText = DoExit();
                        break;

                    case "DoQuit":
                        MainClass.output.MatchMade = true;
                        MainClass.output.OutputText = DoQuit();
                        break;
                }
            }
        }

        private string DoQuit()
        {
            CacheManager.ClearCache();
            MainClass.adminEnabled = false;
            return InitFunctions.GetInitMessage(false);
        }

        public string DoExit()
        {
            return "DoExit";
        }
    }
}
