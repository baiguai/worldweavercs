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

        public static Output output = new Output();
        public static string userInput;
        public static string gameDb = "";
        public static string gameFile = "";

        public static void Main(string[] args)
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            Listener(Tools.InitFunctions.GetInitMessage());
        }

        private static void Listener(string initMsg)
        {
            if (initMsg.Equals(""))
            {
                Console.Write(">> ");
            }
            else
            {
                Console.Write(initMsg);
            }
            userInput = Console.ReadLine();

            globalParser.ParseInput();
            if (output.MatchMade && output.OutputText.Equals("DoExit"))
            {
                Environment.Exit(0);
            }


            if (!output.MatchMade)
            {
                adminParser.ParseInput();
            }

            if (!output.MatchMade)
            {
                gameParser.ParseInput();
            }

            if (output.MatchMade)
            {
                if (output.OutputText.Equals(""))
                {
                    output.OutputText = AppSettingFunctions.GetConfigValue("messages", "unknown_command");
                }

                Console.Clear();
                Console.WriteLine(output.OutputText);
                Console.WriteLine("");
                Console.WriteLine("");

                if (Cache.FightCache.Fight != null)
                {
                    var elemDb = new DataManagement.GameLogic.Element();
                    var playerLife = Tools.Elements.GetLife(Cache.PlayerCache.Player).ToString("N0");
                    var enemyLife = Tools.Elements.GetLife(Cache.FightCache.Fight.Enemy).ToString("N0");
                    Console.WriteLine($"Enemy Life: {enemyLife}");
                    Console.WriteLine($"Player Life: {playerLife}");
                    Console.WriteLine("");

                    if (Cache.FightCache.Fight != null && !Cache.FightCache.Fight.PlayersTurn)
                    {
                        Thread.Sleep(2000);
                        var attParser = new Parsers.Elements.Attack();
                        attParser.ProcessFightRound();

                        Console.WriteLine(output.OutputText);
                        Console.WriteLine("");
                        Console.WriteLine("");

                        playerLife = Tools.Elements.GetLife(Cache.PlayerCache.Player).ToString("N0");
                        enemyLife = Tools.Elements.GetLife(Cache.FightCache.Fight.Enemy).ToString("N0");
                        Console.WriteLine($"Enemy Life: {enemyLife}");
                        Console.WriteLine($"Player Life: {playerLife}");
                        Console.WriteLine("");
                    }
                    else
                    {
                        if (Cache.FightCache.Fight.PlayerFleeing)
                        {
                            Cache.FightCache.Fight = null;
                        }
                    }
                }
            }

            Listener("");
        }
    }
}
