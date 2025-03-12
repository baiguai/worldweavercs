using System;
using System.Diagnostics.Tracing;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Set
    {
        public void ParseSet(Classes.Element parentElement, Classes.Element currentElement)
        {
            var setPerformed = false;

            if (currentElement.Tags.TagsContain("arm"))
            {
                SetArmed();
            }

            if (!setPerformed)
            {
                setPerformed = SetElementChildValueByTag(
                    currentElement,
                    currentElement.Tags,
                    currentElement.Logic,
                    currentElement.Output
                );
            }
            SetElementValue(
                currentElement,
                currentElement.Tags,
                currentElement.Logic,
                currentElement.Output
            );
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementChildValueByTag(
                    currentElement,
                    currentElement.Tags,
                    currentElement.Logic,
                    currentElement.Output
                );
            }
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementChildrenValueByTag(
                    currentElement,
                    currentElement.Tags,
                    currentElement.Logic,
                    currentElement.Output
                );
            }
            if (!setPerformed)
            {
                setPerformed = SetRelativeElementValue(
                    currentElement,
                    currentElement.Tags,
                    currentElement.Logic,
                    currentElement.Output
                );
            }

            return;
        }

        private void SetArmed()
        {
            var weaponName = MainClass.userInput.Replace("arm ", "");
            var elemDb = new DataManagement.GameLogic.Element();
            var weapon = elemDb.GetChildElementKeysBySyntax(Cache.PlayerCache.Player, weaponName, true);

            if (weapon.Equals("") || weapon.Count != 1)
            {
                MainClass.output.OutputText = "Could not locate the weapon you wish to arm.";
                MainClass.output.MatchMade = true;
                return;
            }

            var weaponElem = elemDb.GetElementByKey(weapon.First());

            if (!weaponElem.Tags.TagsContain("weapon"))
            {
                weapon = null;
            }

            foreach (var child in Cache.PlayerCache.Player.Children)
            {
                if (child.Tags.TagsContain("armed"))
                {
                    elemDb.SetElementField(child.ElementKey, "output", weapon.First(), true);
                    MainClass.output.OutputText = $"You are now armed with the {weaponElem.Name}.";
                    MainClass.output.MatchMade = true;
                }
            }
        }

        public void SetDefaultArmed()
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var playerWeapon = Cache.PlayerCache.Player.ChildByTag("weapon");

            if (playerWeapon == null)
            {
                return;
            }

            foreach (var child in Cache.PlayerCache.Player.Children)
            {
                if (child.Tags.TagsContain("armed"))
                {
                    elemDb.SetElementField(child.ElementKey, "output", playerWeapon.ElementKey, true);
                    MainClass.output.OutputText = $"You are now armed with the {playerWeapon.Name}.";
                    MainClass.output.MatchMade = true;
                }
            }
        }

        private bool SetElementValue(
            Classes.Element currentElement,
            string tags,
            string logic,
            string output
        )
        {
            if (!logic.Contains("(") || !logic.Contains(")"))
            {
                return false;
            }

            if (logic.EndsWith(")"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split(")");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");
            var elemDb = new DataManagement.GameLogic.Element();

            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }

            elemDb.SetElementField(key, prop, ProcessOutput(output.Trim(), key));
            return true;
        }

        private bool SetElementChildValueByTag(
            Classes.Element currentElement,
            string tags,
            string logic,
            string output
        )
        {
            if (!logic.Contains("((") || !logic.Contains("))"))
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var key = arr[0].Trim().Replace("(", "").Replace(")", "");

            var elemDb = new DataManagement.GameLogic.Element();
            var elem = elemDb.GetElementByKey(key);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }
            var children = elem.Children.Where(c => c.Tags.TagsContain(tag));
            if (children.Count() < 1)
            {
                return false;
            }

            foreach (var child in children)
            {
                if (!child.ElementKey.Equals(""))
                {
                    elemDb.SetElementField(
                        child.ElementKey,
                        prop,
                        ProcessOutput(output.Trim(), child.ElementKey)
                    );
                }
            }
            return true;
        }

        private bool SetRelativeElementValue(
            Classes.Element currentElement,
            string tags,
            string logic,
            string output
        )
        {
            if (!logic.Contains("[") || !logic.Contains("]"))
            {
                return false;
            }

            if (logic.EndsWith("]"))
            {
                logic = logic = "output";
            }

            var arr = logic.Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var elem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (elem.ElementKey.Equals(""))
            {
                return false;
            }

            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetElementField(
                elem.ElementKey,
                prop,
                ProcessOutput(output.Trim(), elem.ElementKey)
            );
            return true;
        }

        private bool SetRelativeElementChildValueByTag(
            Classes.Element currentElement,
            string tags,
            string logic,
            string output
        )
        {
            if (
                !logic.Contains("((")
                || !logic.Contains("))")
                || !logic.Contains("[")
                || !logic.Contains("]")
            )
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            if (tag.Equals(""))
            {
                tag = tags;
            }
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return false;
            }
            var targetElems = parentElem
                .Children.Where(c => c.Tags.TagsContain(tag));
            var elemDb = new DataManagement.GameLogic.Element();
            if (targetElems.Count() > 0)
            {
                foreach (var targetElem in targetElems)
                {
                    elemDb.SetElementField(
                        targetElem.ElementKey,
                        prop,
                        ProcessOutput(output.Trim(), targetElem.ElementKey)
                    );
                }
                return true;
            }

            return false;
        }

        private string ProcessOutput(string outputValue, string targetElementKey)
        {
            var adjOutput = outputValue;
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = elemDb.GetElementByKey(targetElementKey);
            var relOut = SetRelativeElementChildValueByTag(elem, elem.Tags, outputValue, adjOutput);

            if (outputValue.StartsWith("++"))
            {
                try
                {
                    var outInt = Convert.ToInt32(elem.Output);
                    var res = outInt++;
                    return res.ToString();
                }
                catch (Exception)
                {
                    return elem.Output;
                }
            }
            if (outputValue.StartsWith("--"))
            {
                try
                {
                    var outInt = Convert.ToInt32(elem.Output);
                    var res = outInt--;
                    return res.ToString();
                }
                catch (Exception)
                {
                    return elem.Output;
                }
            }
            if (outputValue.StartsWith("+="))
            {
                try
                {
                    var outInt = Convert.ToInt32(elem.Output);
                    var adj = 0;
                    try
                    {
                        adj = Convert.ToInt32(outputValue.Replace("+=", ""));
                    }
                    catch (Exception) { }
                    var res = outInt + adj;
                    return res.ToString();
                }
                catch (Exception)
                {
                    return elem.Output;
                }
            }
            if (outputValue.StartsWith("-="))
            {
                try
                {
                    var outInt = Convert.ToInt32(elem.Output);
                    var adj = 0;
                    try
                    {
                        adj = Convert.ToInt32(outputValue.Replace("-=", ""));
                    }
                    catch (Exception) { }
                    var res = outInt + (-adj);
                    return res.ToString();
                }
                catch (Exception)
                {
                    return elem.Output;
                }
            }

            return adjOutput;
        }

        private bool SetRelativeElementChildrenValueByTag(
            Classes.Element currentElement,
            string tags,
            string logic,
            string output
        )
        {
            if (
                !logic.Contains("((")
                || !logic.Contains("))")
                || !logic.Contains("[")
                || !logic.Contains("]")
            )
            {
                return false;
            }

            if (logic.EndsWith("))"))
            {
                logic = logic += "output";
            }

            var arr = logic.Split("))");
            if (arr.Length != 2)
            {
                return false;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Trim().Split("]");
            if (arr.Length != 2)
            {
                return false;
            }

            var tag = arr[1].Trim().Replace("((", "").Replace("))", "");
            var relCode = $"[{arr[0].Trim().Replace("[", "").Replace("]", "")}]";

            var parentElem = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (parentElem.ElementKey.Equals(""))
            {
                return false;
            }
            var targetElems = parentElem.Children.Where(c => c.Tags.TagsContain(tags));
            var elemDb = new DataManagement.GameLogic.Element();
            foreach (var child in targetElems)
            {
                elemDb.SetElementField(child.ElementKey, prop, output);
            }

            return true;
        }
    }
}
