using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class FightParser
    {
        public Classes.Output ProcessFightInput(string gameKey, string gameDb, Classes.Output output, string userInput)
        {
            if (Cache.FightCache.Fight == null)
            {
                return output;
            }

            var attk = new Parsers.Elements.Attack();
            return attk.ProcessFightRound(gameDb, output, userInput);
        }
    }
}