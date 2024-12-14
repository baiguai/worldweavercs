using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Attack
    {
        public void ParseAttack(Classes.Element parentElement, Classes.Element currentElement)
        {
            var elemParser = new Elements.Element();
            var newFight = false;
            var elemDb = new DataManagement.GameLogic.Element();
            var attackables = elemDb.GetElementsByTag("attackable");
            var target = Tools.Elements.GetRelativeElement(currentElement, currentElement.AttributeByTag("target").Output);
            var playersTurn = true;
            var playerWeapon = Cache.PlayerCache.Player.AttributeByTag("armed");

            if (attackables.Count() < 1)
            {
                return;
            }
            if (target == null)
            {
                return;
            }

            if (playerWeapon == null)
            {
                playersTurn = false;
            }

            if (target.ElementKey.Equals(Cache.PlayerCache.Player.ElementKey))
            {
                target = Tools.Elements.GetSelf(currentElement);
                playersTurn = false;
            }

            if (Cache.FightCache.Fight == null) 
            {
                Cache.FightCache.Fight = new Classes.Fight();
                Cache.FightCache.Fight.Enemies.AddRange(attackables);
            }

            Cache.FightCache.Fight.PlayersTurn = playersTurn;
            Cache.FightCache.Fight.Target = target;

            ProcessFightRound();

            return;
        }


        public void ProcessFightRound()
        {
            if (Cache.FightCache.Fight.PlayersTurn)
            {
                var playerWeapon = Cache.PlayerCache.Player.AttributeByTag("armed");
                var attackRoll = Tools.ValueTools.Randomize(1, 20);
                var enemyArmor = Cache.FightCache.Fight.Target.AttributeByTag("armor");

                MainClass.output.OutputText += $"Player attack roll: {attackRoll}{Environment.NewLine}Enemy's armor rating: {enemyArmor.Output}{Environment.NewLine}{Environment.NewLine}";

                if (Convert.ToInt32(enemyArmor.Output) <= attackRoll)
                {
                    var damageMsg = Cache.FightCache.Fight.Target.ChildByType("damage_message");
                    var gameLgc = new DataManagement.GameLogic.Element();
                    var enemyLife = Cache.FightCache.Fight.Target.AttributeByTag("life");
                    var damageAttrib = playerWeapon.AttributeByTag("damage");
                    var damage = Convert.ToInt32(damageAttrib.Logic.RandomValue(damageAttrib));
                    var newLifeValue = Convert.ToInt32(enemyLife.Output) - damage;

                    if (damageMsg != null)
                    {
                        var dmgMsg = ProcessDamageOutput(damageMsg.Output, damage);
                        MainClass.output.OutputText += dmgMsg;
                    }

                    if (newLifeValue < 1)
                    {
                        var actn = new Parsers.Elements.Action();
                        MainClass.output.MatchMade = true;
                        actn.DoKill();
                        return;
                    }

                    gameLgc.SetElementField(enemyLife.ElementKey, "Output", newLifeValue.ToString());
                    Tools.CacheManager.RefreshFightCache();
                }
                else
                {
                    var missElem = Cache.FightCache.Fight.Target.ChildByType("miss_message");
                    var missMsg = ProcessDamageOutput(missElem.Output, 0);
                    MainClass.output.OutputText = Tools.OutputProcessor.ProcessOutputText(missMsg, missElem);
                }

                var allDead = true;
                foreach (var enemy in Cache.FightCache.Fight.Enemies)
                {
                    var elife = Convert.ToInt32(enemy.AttributeByTag("life"));
                    if (elife > 0)
                    {
                        allDead = false;
                        break;
                    }
                }

                MainClass.output.MatchMade = true;

                Cache.FightCache.Fight.PlayersTurn = false;

                if (allDead)
                {
                    Cache.FightCache.Fight = null;
                    MainClass.output.OutputText += Environment.NewLine + "You've vanquished your enemies.";
                }
            }
            else
            {
                MainClass.output.OutputText = "";

                if (Cache.FightCache.Fight.PlayerFleeing)
                {
                    MainClass.output.OutputText = Tools.AppSettingFunctions.GetConfigValue("messages", "flee_message") + Environment.NewLine;
                }

                var enemyWeapon = Cache.FightCache.Fight.Target.AttributeByTag("armed");
                if (enemyWeapon != null)
                {
                    enemyWeapon = Cache.FightCache.Fight.Target.ChildByKey(enemyWeapon.Output);
                    var attackRoll = Tools.ValueTools.Randomize(1, 20);
                    var playerArmor = Cache.PlayerCache.Player.AttributeByTag("armor");

                    MainClass.output.OutputText += $"Enemy attack roll: {attackRoll}{Environment.NewLine}Player's armor rating: {playerArmor.Output}{Environment.NewLine}{Environment.NewLine}";

                    if (Convert.ToInt32(playerArmor.Output) <= attackRoll)
                    {
                        var gameLgc = new DataManagement.GameLogic.Element();
                        var playerLife = Cache.PlayerCache.Player.AttributeByTag("life");
                        var damageAttrib = enemyWeapon.AttributeByTag("damage");
                        var damage = Convert.ToInt32(damageAttrib.Logic.RandomValue(damageAttrib));
                        var newLifeValue = Convert.ToInt32(playerLife.Output) - damage;

                        var damageOutput = Cache.FightCache.Fight.Target.ChildByType("hit");
                        if (damageOutput != null)
                        {
                            var dmgMsg = ProcessDamageOutput(damageOutput.Output, damage);
                            MainClass.output.OutputText += dmgMsg;
                        }

                        if (newLifeValue < 1)
                        {
                            var actn = new Parsers.Elements.Action();
                            actn.DoDie();
                            return;
                        }

                        gameLgc.SetElementField(playerLife.ElementKey, "Output", newLifeValue.ToString());

                        MainClass.output.MatchMade = true;
                    }
                    else
                    {
                        var missElem = Cache.FightCache.Fight.Target.ChildByType("miss");
                        var missMsg = ProcessDamageOutput(missElem.Output, 0);
                        MainClass.output.OutputText = Tools.OutputProcessor.ProcessOutputText(missMsg, missElem);
                    }
                }

                Cache.FightCache.Fight.PlayersTurn = true;
            }

            return;
        }

        private string ProcessDamageOutput(string damageOutput, int damage)
        {
            var convOutput = damageOutput;
            convOutput = convOutput.Replace("[damage]", damage.ToString());

            return convOutput + Environment.NewLine;
        }
    }
}
