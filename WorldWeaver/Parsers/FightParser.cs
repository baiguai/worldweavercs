using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class FightParser
    {
        public Classes.Output ProcessFightInput(string gameKey, string gameDb, Classes.Output output, string userInput)
        {
            var method = Tools.CommandFunctions.GetCommandMethod(userInput, "FightParser");
            var duringGame = Tools.CommandFunctions.GetDuringGameOption(userInput, "FightParser");

            if (method.Equals("DoFlee") && duringGame && Cache.FightCache.Fight != null)
            {
                var enemy = Cache.FightCache.Fight.Enemy;
                var elemTypes = new List<string>();
                elemTypes.Add("npc");
                var childProcs = new List<string>();
                childProcs.Add("attack");
                var enemProc = new ElementProc()
                {
                    CurrentElementTypes = elemTypes,
                    ChildProcElements = childProcs,
                    AllowRepeatOptions = false
                };

                var elemParser = new Parsers.Elements.Element();
                output = elemParser.ParseElement(output, gameDb, enemy, userInput, enemProc, false);
            }

            return output;
        }
    }
}