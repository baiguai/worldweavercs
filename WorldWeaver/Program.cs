using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver
{
    class MainClass
    {
        private static Parsers.GlobalParser globalParser = new Parsers.GlobalParser();
        private static Parsers.AdminParser adminParser = new Parsers.AdminParser();
        private static Parsers.GameParser gameParser = new Parsers.GameParser();
        public static Logger logger = new Logger() { LogDate = DateTime.Now };

        public static void Main(string[] args)
        {
            // Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Listener(Tools.InitFunctions.GetInitMessage());
        }

        private static void Listener(string initMsg)
        {
            Output output;

            if (initMsg.Equals(""))
            {
                Console.Write(">> ");
            }
            else
            {
                Console.Write(initMsg);
            }
            string input = Console.ReadLine();

            output = globalParser.ParseInput(input);
            if (output.MatchMade && output.OutputText.Equals("DoExit"))
            {
                Environment.Exit(0);
            }


            if (!output.MatchMade)
            {
                output = adminParser.ParseInput(input);
            }

            if (!output.MatchMade)
            {
                output = gameParser.ParseInput(input);
            }


            if (output.MatchMade)
            {
                Console.Clear();
                Console.WriteLine(output.OutputText);
                Console.WriteLine("");
                Console.WriteLine("");
            }

            Listener("");
        }
    }
}
