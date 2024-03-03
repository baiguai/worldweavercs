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

        public Classes.Output ParseInput(string input)
        {
            var output = new Output();
            var method = Tools.CommandFunctions.GetCommandMethod(input, "GlobalParser");

            if (!method.Equals(""))
            {
                playerInput = input;
                output.MatchMade = true;

                switch (method)
                {
                    case "DoExit":
                        output.MatchMade = true;
                        output.OutputText = DoExit();
                        break;

                    case "DoHelp":
                        output.MatchMade = true;
                        output.OutputText = DoHelp();
                        break;
                }
            }

            return output;
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
