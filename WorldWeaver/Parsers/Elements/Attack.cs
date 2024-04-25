using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Attack
    {
        public Classes.Output ParseAttack(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, string userInput)
        {
            var elemParser = new Elements.Element();
            var newFight = false;
            Classes.Element? init = null;

            if (Cache.FightCache.Fight == null)
            {
                Cache.FightCache.Fight = new Classes.Fight();
                var target = currentElement.AttributeByTag("target");

                if (target.Output.Equals("[self]") && currentElement.ElementKey != Cache.PlayerCache.Player.ElementKey)
                {
                    target = Tools.Elements.GetSelf(gameDb, currentElement);
                }

                init = currentElement.AttributeByTag("initiative");
                if (target == null)
                {
                    Cache.FightCache.Fight = null;
                    output.MatchMade = true;
                    return output;
                }

                Cache.FightCache.Fight.Enemy = target;
                Cache.FightCache.Fight.PlayersTurn = (init.ElementType != "player");

                newFight = true;
            }

            if (newFight)
            {
                if (init == null || init.Output.Equals("[player]"))
                {
                    Cache.FightCache.Fight.PlayersTurn = true;
                }
                else
                {
                    Cache.FightCache.Fight.PlayersTurn = false;
                }
            }


            output = ProcessFightRound(gameDb, output, userInput);

            return output;
        }


        public Output ProcessFightRound(string gameDb, Output output, string userInput) // @todo
        {
            var method = Tools.CommandFunctions.GetCommandMethod(userInput, "FightParser");

            if (method.Equals("DoFlee"))
            {
                output.OutputText = Tools.AppSettingFunctions.GetConfigValue("messages", "flee_message") + Environment.NewLine;
                Cache.FightCache.Fight.PlayersTurn = false;
            }

            if (Cache.FightCache.Fight.PlayersTurn)
            {
                var playerWeapon = Cache.PlayerCache.Player.AttributeByTag("armed");
                if (playerWeapon == null || playerWeapon.Output.Equals(""))
                {
                    output.OutputText += Tools.AppSettingFunctions.GetConfigValue("messages", "unarmed");
                    output.MatchMade = true;
                    Cache.FightCache.Fight.PlayersTurn = false;
                    return output;
                }
                else
                {
                    playerWeapon = Cache.PlayerCache.Player.ChildByKey(playerWeapon.Output);
                    var attackRoll = Tools.ValueTools.Randomize(1, 20);
                    var enemyArmor = Cache.FightCache.Fight.Enemy.AttributeByTag("armor");

                    output.OutputText += $"Player attack roll: {attackRoll}{Environment.NewLine}Enemy's armor rating: {enemyArmor.Output}";

                    if (Convert.ToInt32(enemyArmor.Output) <= attackRoll)
                    {
                        var damageMsg = Cache.FightCache.Fight.Enemy.ChildByType("damage_message");
                        var gameLgc = new DataManagement.GameLogic.Element();
                        var enemyLife = Cache.FightCache.Fight.Enemy.AttributeByTag("life");
                        var damageAttrib = playerWeapon.AttributeByTag("damage");
                        var damage = damageAttrib.Logic.RandomValue();
                        var newLifeValue = Convert.ToInt32(enemyLife.Output) - damage;

                        if (damageMsg != null)
                        {
                            var dmgMsg = ProcessEnemyDamageOutput(damageMsg.Output, damage);
                            output.OutputText += dmgMsg;
                        }

                        if (newLifeValue < 1)
                        {
                            var actn = new Parsers.Elements.Action();
                            output.MatchMade = true;
                            return actn.DoKill(gameDb, output, userInput);
                        }

                        gameLgc.SetElementField(gameDb, enemyLife.ElementKey, "Output", newLifeValue.ToString());
                    }
                    else
                    {
                        var missElem = Cache.FightCache.Fight.Enemy.ChildByType("miss_message");
                        var missMsg = ProcessEnemyDamageOutput(missElem.Output, 0);
                        output.OutputText = missMsg;
                    }

                    output.MatchMade = true;
                }

                Cache.FightCache.Fight.PlayersTurn = false;
            }
            else
            {
                output.OutputText = "";
                var enemyWeapon = Cache.FightCache.Fight.Enemy.AttributeByTag("armed");
                if (enemyWeapon != null)
                {
                    enemyWeapon = Cache.FightCache.Fight.Enemy.ChildByKey(enemyWeapon.Output);
                    var attackRoll = Tools.ValueTools.Randomize(1, 20);
                    var playerArmor = Cache.PlayerCache.Player.AttributeByTag("armor");

                    output.OutputText += $"Enemy attack roll: {attackRoll}{Environment.NewLine}Player's armor rating: {playerArmor.Output}{Environment.NewLine}";

                    if (Convert.ToInt32(playerArmor.Output) <= attackRoll)
                    {
                        var gameLgc = new DataManagement.GameLogic.Element();
                        var playerLife = Cache.PlayerCache.Player.AttributeByTag("life");
                        var damageAttrib = enemyWeapon.AttributeByTag("damage");
                        var damage = damageAttrib.Logic.RandomValue();
                        var newLifeValue = Convert.ToInt32(playerLife.Output) - damage;

                        var damageOutput = enemyWeapon.ChildByTag("hit");
                        if (damageOutput != null)
                        {
                            var dmgMsg = ProcessPlayerDamageOutput(damageOutput.Output, damage);
                            output.OutputText += dmgMsg;
                        }

                        if (newLifeValue < 1)
                        {
                            var actn = new Parsers.Elements.Action();
                            return actn.DoDie(gameDb, output, userInput);
                        }

                        gameLgc.SetElementField(gameDb, playerLife.ElementKey, "Output", newLifeValue.ToString());

                        output.MatchMade = true;
                    }
                }

                Cache.FightCache.Fight.PlayersTurn = true;
            }

            if (method.Equals("DoFlee"))
            {
                Cache.FightCache.Fight = null;
            }

            return output;
        }

        private string ProcessEnemyDamageOutput(string output, int damage)
        {
            var convOutput = output;
            convOutput = convOutput.Replace("[enemy.title]", Cache.FightCache.Fight.Enemy.AttributeByTag("title").Output);
            convOutput = convOutput.Replace("[enemy.subject_pronoun]", Cache.FightCache.Fight.Enemy.AttributeByTag("subject_pronoun").Output);
            convOutput = convOutput.Replace("[damage]", damage.ToString());

            return convOutput + Environment.NewLine;
        }

        private object ProcessPlayerDamageOutput(string output, int damage)
        {
            var convOutput = output;
            convOutput = convOutput.Replace("[self.name]", Cache.FightCache.Fight.Enemy.Name);
            convOutput = convOutput.Replace("[damage]", damage.ToString());

            return convOutput + Environment.NewLine;
        }
    }
}
