using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Attack
    {
        public Classes.Output ParseAttack(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            output.MatchMade = false;
            var elemParser = new Elements.Element();

            Cache.FightCache.Fight = new Classes.Fight();

            Regex rgx = new Regex(currentElement.Syntax, RegexOptions.IgnoreCase);

            if (rgx.IsMatch(userInput))
            {
                //
            }

            return output;
        }
    }
}
