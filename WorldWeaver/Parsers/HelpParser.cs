using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class HelpParser
    {
        public void ParseInput()
        {
            MainClass.output.MatchMade = false;
            MainClass.output.OutputText = "";
            var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "HelpParser");

            if (!method.Equals(""))
            {
                var playerInput = MainClass.userInput;

                switch (method)
                {
                    case "DoHelp":
                        if (MainClass.gameDb.Equals(""))
                        {
                            return;
                        }
                        MainClass.output.OutputText = DoHelp(playerInput);
                        if (!MainClass.output.OutputText.Equals(""))
                        {
                            MainClass.output.MatchMade = true;
                        }
                        break;

                    case "DoHelpIndex":
                        if (MainClass.gameDb.Equals(""))
                        {
                            return;
                        }
                        MainClass.output.OutputText = DoHelpIndex(playerInput);
                        if (!MainClass.output.OutputText.Equals(""))
                        {
                            MainClass.output.MatchMade = true;
                        }
                        break;
                }
            }
        }

        private string DoHelp(string playerInput)
        {
            var helpOutput = "";
            DataManagement.GameLogic.Help helpDb = new DataManagement.GameLogic.Help();
            var rel = "";

            var topics = helpDb.GetHelpArticles(playerInput);

            foreach (var hlp in topics)
            {
                rel = "";

                if (!helpOutput.Equals(""))
                {
                    helpOutput += Environment.NewLine + Environment.NewLine;
                }
                helpOutput += hlp.Title + Environment.NewLine;
                helpOutput += "---" + Environment.NewLine;
                helpOutput += hlp.Article;
                if (hlp.Related.Count() > 0 && !hlp.Related.First().Equals(""))
                {
                    helpOutput += Environment.NewLine + Environment.NewLine + "---" + Environment.NewLine;
                    helpOutput += "Related:" + Environment.NewLine;

                    foreach (var reltd in hlp.Related)
                    {
                        if (!rel.Equals(""))
                        {
                            rel += Environment.NewLine;
                        }
                        rel += reltd;
                    }

                    helpOutput += rel;
                }
            }

            return helpOutput;
        }

        private string DoHelpIndex(string playerInput)
        {
            playerInput = playerInput.Replace("? index", "").Trim();
            var helpOutput = "";
            var letterOutput = "";
            DataManagement.GameLogic.Help helpDb = new DataManagement.GameLogic.Help();
            var topics = helpDb.GetHelpArticles(playerInput);

            for (char letter = 'a'; letter <= 'z'; letter++)
            {
                if (!helpOutput.Equals(""))
                {
                    helpOutput += Environment.NewLine + Environment.NewLine;
                }

                letterOutput = "";
                foreach (var topic in topics)
                {
                    if (playerInput.Equals("") || playerInput.StartsWith(letter.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (topic.Title.StartsWith(letter.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            if (letterOutput.Equals(""))
                            {
                                letterOutput = letter.ToString().ToUpper();
                            }
                            if (!letterOutput.Equals(""))
                            {
                                letterOutput += Environment.NewLine;
                            }
                            letterOutput += topic.Title;
                        }
                    }
                }

                helpOutput += letterOutput;
            }

            return helpOutput;
        }
    }
}