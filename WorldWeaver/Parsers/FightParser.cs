using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class FightParser
    {
        public void ProcessFightInput()
        {
            if (Cache.FightCache.Fight == null)
            {
                return;
            }

            var attk = new Parsers.Elements.Attack();
            attk.ProcessFightRound();
        }
    }
}