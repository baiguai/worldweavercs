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
            var attackables = elemDb.GetRoomElementsByTag("attackable", Cache.RoomCache.Room.ElementKey);
            var target = Tools.Elements.GetRelativeElement(currentElement, currentElement.AttributeByTag("target").Output);
            var playersTurn = true;
            var playerWeapon = Cache.PlayerCache.Player.AttributeByTag("armed");

            if (MainClass.macro.IsRecording || MainClass.macro.IsRunning)
            {
                return;
            }

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
                MainClass.output.OutputText += $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}!FIGHT!{Environment.NewLine}";
            }

            Cache.FightCache.Fight.PlayersTurn = playersTurn;
            Cache.FightCache.Fight.Target = target;

            ProcessFightRound();

            return;
        }


        public void ProcessFightRound()
        {
            var elemDb = new DataManagement.GameLogic.Element();

            if (Cache.FightCache.Fight.PlayersTurn)
            {
                MainClass.output.OutputText = "";

                var playerWeapon = elemDb.GetElementByKey(Cache.PlayerCache.Player.AttributeByTag("armed").Output);
                if (playerWeapon == null)
                {
                    MainClass.output.OutputText = "You aren't armed with a weapon.";
                    MainClass.output.MatchMade = true;
                    return;
                }

                ProcessAttackEvent(playerWeapon, "!_canattack");
                if (MainClass.output.FailedLogic)
                {
                    return;
                }

                var attackRoll = Tools.ValueTools.Randomize(1, 20);
                var enemyArmor = Cache.FightCache.Fight.Target.AttributeByTag("armor");

                MainClass.output.OutputText += $"Player attack roll: {attackRoll}{Environment.NewLine}Enemy's armor rating: {enemyArmor.Output}{Environment.NewLine}";

                if (Convert.ToInt32(enemyArmor.Output) <= attackRoll)
                {
                    var damageMsg = Cache.FightCache.Fight.Target.ChildByType("damage_message");
                    var gameLgc = new DataManagement.GameLogic.Element();
                    var enemyLife = Cache.FightCache.Fight.Target.AttributeByTag("life");
                    var damageAttrib = playerWeapon.AttributeByTag("damage");

                    int damage = 0;
                    if (damageAttrib == null)
                    {
                        // The player doesn't have a weapon, so do low - fist - damage.
                        damage = FormatExtensions.RollDice("[roll:1d4]", Cache.PlayerCache.Player);
                    }
                    else
                    {
                        damage = Convert.ToInt32(damageAttrib.Logic.RandomValue(damageAttrib));
                    }
                    var newLifeValue = Convert.ToInt32(enemyLife.Output) - damage;

                    if (newLifeValue < 0)
                    {
                        newLifeValue = 0;
                    }
                    gameLgc.SetElementField(enemyLife.ElementKey, "Output", newLifeValue.ToString());
                    Tools.CacheManager.RefreshFightCache();

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

                    Cache.FightCache.Fight.RoundHandled = true;
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
                    var elife = Convert.ToInt32(enemy.AttributeByTag("life").Output);
                    if (elife > 0)
                    {
                        allDead = false;
                        break;
                    }
                }

                MainClass.output.MatchMade = true;
                MainClass.output.OutputText += Environment.NewLine;
                Cache.FightCache.Fight.InitialRound = false;

                ProcessAttackEvent(playerWeapon, "!_attack");

                Cache.FightCache.Fight.PlayersTurn = false;

                if (allDead)
                {
                    Cache.FightCache.Fight = null;
                    MainClass.output.OutputText += Environment.NewLine + "You've vanquished your enemies.";
                }
            }
            else
            {
                if (Cache.FightCache.Fight.PlayerFleeing)
                {
                    MainClass.output.OutputText = Tools.AppSettingFunctions.GetConfigValue("messages", "flee_message") + Environment.NewLine;
                    MainClass.output.MatchMade = true;
                }
                var enemyLife = Cache.FightCache.Fight.Target.AttributeByTag("life");
                var attackAdj = Cache.FightCache.Fight.Target.AttributeByTag("!_attackadjust");

                if (enemyLife == null || Convert.ToInt32(enemyLife.Output) < 1)
                {
                    return;
                }

                var enemyWeapon = Cache.FightCache.Fight.Target.AttributeByTag("armed");

                if (enemyWeapon == null)
                {
                    Cache.FightCache.Fight.PlayersTurn = true;
                    Cache.FightCache.Fight.RoundHandled = true;
                    return;
                }

                ProcessAttackEvent(enemyWeapon, "!_canattack");
                if (MainClass.output.FailedLogic)
                {
                    return;
                }

                var attackRoll = Tools.ValueTools.Randomize(1, 20);

                try
                {
                    var attackAdjInt = Convert.ToInt32(attackAdj.Output);
                    attackRoll = attackRoll + attackAdjInt;
                }
                catch (Exception)
                {
                    // Don't make any adjustments
                }
                var playerArmor = Cache.PlayerCache.Player.AttributeByTag("armor");

                MainClass.output.OutputText += $"{Environment.NewLine}Enemy attack roll: {attackRoll}{Environment.NewLine}Player's armor rating: {playerArmor.Output}{Environment.NewLine}";

                if (Convert.ToInt32(playerArmor.Output) <= attackRoll)
                {
                    var gameLgc = new DataManagement.GameLogic.Element();
                    var playerLife = Cache.PlayerCache.Player.AttributeByTag("life");
                    var damageAttrib = enemyWeapon.AttributeByTag("damage");
                    var damage = 0;
                    var newLifeValue = 1;
                    
                    if (playerLife != null && damageAttrib != null)
                    {
                        damage = Convert.ToInt32(damageAttrib.Logic.RandomValue(damageAttrib));
                        newLifeValue = Convert.ToInt32(playerLife.Output) - damage;
                    }

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

                    ProcessAttackEvent(enemyWeapon, "!_attack");

                    MainClass.output.MatchMade = true;
                }
                else
                {
                    var missElem = Cache.FightCache.Fight.Target.ChildByType("miss");
                    if (missElem != null)
                    {
                        var missMsg = ProcessDamageOutput(missElem.Output, 0);
                        MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(missMsg, missElem);
                    }
                }

                Cache.FightCache.Fight.PlayersTurn = true;
                Cache.FightCache.Fight.RoundHandled = true;
                MainClass.output.OutputText += Environment.NewLine;
            }

            return;
        }

        private void ProcessAttackEvent(Classes.Element weapon, string tag)
        {
            DataManagement.GameLogic.Element elemDb = new DataManagement.GameLogic.Element();
            Parsers.Elements.Logic lgcObj = new Parsers.Elements.Logic();
            Parsers.Elements.Set setObj = new Parsers.Elements.Set();
            Parsers.Elements.Message msgObj = new Parsers.Elements.Message();
            if (weapon == null)
            {
                return;
            }

            var attackElems = weapon.ChildrenByTag(tag);

            foreach (var elem in attackElems)
            {
                foreach (var child in elem.Children)
                {
                    if (child.ElementType.Equals("logic", StringComparison.OrdinalIgnoreCase))
                    {
                        lgcObj.ParseLogic(child);
                    }
                    if (child.ElementType.Equals("set", StringComparison.OrdinalIgnoreCase))
                    {
                        setObj.ParseSet(elem, child);
                    }
                    if (child.ElementType.Equals("message", StringComparison.OrdinalIgnoreCase))
                    {
                        msgObj.ParseMessage(elem, child, false, 0);
                    }
                }
            }
        }

        private string ProcessDamageOutput(string damageOutput, int damage)
        {
            var convOutput = damageOutput;
            convOutput = convOutput.Replace("[damage]", damage.ToString());

            return convOutput + Environment.NewLine;
        }
    }
}
