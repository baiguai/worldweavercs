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

        public static Macro macro = new Macro();

        public static Output output = new Output();
        public static string userInput;
        public static string gameDb = "";
        public static string gameFile = "";
        public static bool adminEnabled = false;

        public static bool handledNavigation = false;

        public static List<string> devNotesSearchResults = new List<string>();

        public static List<string> history = new List<string>();

        public static void Main(string[] args)
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Listener(Tools.InitFunctions.GetInitMessage());
        }

        private static void Listener(string initMsg)
        {
            if (initMsg.Equals(""))
            {
                Console.Write(GetCursor());
            }
            else
            {
                Console.Write(initMsg);
            }
            userInput = Console.ReadLine();

            if (macro.IsRecording && !userInput.Equals("_recordoff"))
            {
                macro.MacroSteps.Add(userInput);
            }

            handledNavigation = false;
            output.PlayerMoved = false;
            RunTheParsers(userInput);

            if (output.MatchMade)
            {
                if (output.OutputText.Equals(""))
                {
                    output.OutputText = AppSettingFunctions.GetConfigValue("messages", "unknown_command");
                }

                Console.Clear();
                Console.WriteLine(output.OutputText);
                Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");
                output.ExitFlow = false;

                HandleTheFight();
            }

            Listener("");
        }

        public static void HandleTheFight()
        {
            if (Cache.FightCache.Fight != null)
            {
                var elemDb = new DataManagement.GameLogic.Element();
                var playerLife = Tools.Elements.GetLife(Cache.PlayerCache.Player).ToString("N0");
                var enemyLife = Tools.Elements.GetLife(Cache.FightCache.Fight.Target).ToString("N0");

                if (Cache.FightCache.Fight != null && !Cache.FightCache.Fight.PlayerFleeing)
                {
                    if (Cache.FightCache.Fight.PlayersTurn)
                    {
                        Console.WriteLine($"Enemy Life: {enemyLife}");
                        Console.WriteLine($"Player Life: {playerLife}");
                        Console.WriteLine("");
                    }
                    else
                    {
                        if (Convert.ToInt32(enemyLife) > 0)
                        {
                            MainClass.output.OutputText = "";
                            Thread.Sleep(2000);
                            var attParser = new Parsers.Elements.Attack();
                            attParser.ProcessFightRound();

                            Console.WriteLine(output.OutputText);
                            Console.WriteLine("");
                            Console.WriteLine("");

                            playerLife = Tools.Elements.GetLife(Cache.PlayerCache.Player).ToString("N0");
                            enemyLife = Tools.Elements.GetLife(Cache.FightCache.Fight.Target).ToString("N0");
                            Console.WriteLine($"Enemy Life: {enemyLife}");
                            Console.WriteLine($"Player Life: {playerLife}");
                            Console.WriteLine("");
                        }
                        else
                        {
                            Console.WriteLine($"Enemy Life: {enemyLife}");
                            Console.WriteLine($"Player Life: {playerLife}");
                            Console.WriteLine("");
                            Cache.FightCache.Fight = null;
                        }
                    }
                }
                else
                {
                    if (Cache.FightCache.Fight.PlayerFleeing)
                    {
                        Console.WriteLine($"Enemy Life: {enemyLife}");
                        Console.WriteLine($"Player Life: {playerLife}");
                        Console.WriteLine("");
                        Cache.FightCache.Fight = null;
                    }
                }
            }
        }

        public static void RunTheParsers(string userInputIn)
        {
            userInput = userInputIn;

            globalParser.ParseInput();
            if (output.MatchMade && output.OutputText.Equals("DoExit"))
            {
                Environment.Exit(0);
            }

            if (!output.MatchMade)
            {
                adminParser.ParseInput();
            }

            if (output.ExitFlow)
            {
                return;
            }

            if (!output.MatchMade)
            {
                gameParser.ParseInput();
            }
        }

        private static string GetCursor()
        {
            var curs = "";

            if (adminEnabled)
            {
                curs += "!";
            }

            curs += ">> ";

            return curs;
        }
    }
}
