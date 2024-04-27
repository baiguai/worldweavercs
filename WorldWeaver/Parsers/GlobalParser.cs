using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorldWeaver.Classes;
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
                MainClass.output.MatchMade = true;

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

                    case "DoHelp":
                        MainClass.output.MatchMade = true;
                        MainClass.output.OutputText = DoHelp();
                        break;
                }
            }
        }

        private string DoQuit()
        {
            CacheManager.ClearCache();
            return InitFunctions.GetInitMessage(false);
        }

        public string DoExit()
        {
            return "DoExit";
        }

        public string DoHelp()
        {
            var parms = playerInput.GetInputParams();

            if (parms.Equals(""))
            {
                parms = "help";
            }

            var output = Tools.CommandFunctions.GetHelpTopic(parms, "General");

            return output;
        }
    }
}
