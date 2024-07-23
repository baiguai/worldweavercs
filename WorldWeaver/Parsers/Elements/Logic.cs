using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SQLitePCL;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Logic
    {
        public void ParseLogic(Classes.Element currentElement)
        {
            MainClass.output.FailedLogic = false;
            var passed = true;
            var lines = currentElement.Logic.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            );

            foreach (var line in lines)
            {
                if (line.StartsWith("?"))
                {
                    passed = ParseConditional(currentElement, lines);
                    if (!passed)
                    {
                        MainClass.output.FailedLogic = true;
                        if (!currentElement.Output.Equals(""))
                        {
                            MainClass.output.OutputText += Environment.NewLine + currentElement.Output;
                        }
                        return;
                    }
                    else
                    {
                        MainClass.output.FailedLogic = false;
                        // MainClass.output.OutputText += Tools.OutputProcessor.ProcessOutputText(currentElement.Output, currentElement);
                        return;
                    }
                }
            }

            MainClass.output.FailedLogic = !passed;

            return;
        }

        public bool ParseConditional(Classes.Element currentElement, string line)
        {
            string[] lines = { $"?{line}" };
            return ParseConditional(currentElement, lines);
        }
        public bool ParseConditional(Classes.Element currentElement, string[] lines)
        {
            var passed = true;
            var curCheck = true;
            var operand = "";

            foreach (var line in lines)
            {
                switch (line.ToLower().Trim())
                {
                    case "and":
                        if (!curCheck)
                        {
                            return curCheck;
                        }
                        break;

                    case "or":
                        if (curCheck)
                        {
                            return curCheck;
                        }
                        break;
                }

                if (line.StartsWith("?"))
                {
                    operand = GetOperand(line);
                    var arr = line.Split(operand);
                    if (arr.Length == 2)
                    {
                        var variable1 = GetVariableValue(currentElement, arr[0].Trim());
                        var variable2 = GetVariableValue(currentElement, arr[1].Trim());
                        curCheck = DoComparison(variable1, variable2, operand);
                    }
                }
            }

            passed = curCheck;

            return passed;
        }

        #region Conditional Helpers
        public static string GetOperand(string line)
        {
            var opList = AppSettingFunctions.GetRootArray("Config/LogicOperands.json");

            foreach (var op in opList)
            {
                var arr = line.Split(op);
                if (arr.Length > 1)
                {
                    return op;
                }
            }

            return "";
        }

        private string GetVariableValue(Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.StartsWith("?"))
            {
                rawVariable = rawVariable.Substring(1).Trim();
            }

            if (rawVariable.StartsWith("'"))
            {
                return rawVariable.Replace("'", "");
            }

            if (rawVariable.Equals("[self]"))
            {
                return Tools.Elements.GetSelf(currentElement).ElementKey;
            }

            if (rawVariable.Equals("[room]"))
            {
                return Cache.RoomCache.Room.ElementKey;
            }

            if (rawVariable.Equals("[player]"))
            {
                return Cache.PlayerCache.Player.ElementKey;
            }

            if (rawVariable.Equals("[enemy]"))
            {
                if (Cache.FightCache.Fight == null)
                {
                    return "";
                }
                return Cache.FightCache.Fight.Enemy.ElementKey;
            }

            if (Regex.IsMatch(rawVariable.Trim(), @"^\d+$"))
            {
                return rawVariable.Trim();
            }

            var varValue = "";

            varValue = ParsePresetValues(currentElement, rawVariable);
            if (!varValue.Equals(""))
            {
                return varValue;
            }

            if (rawVariable.StartsWith("ls("))
            {
                rawVariable = rawVariable.Replace("ls(", "(");
                varValue = ListElementChildrenByTag(currentElement, rawVariable);
                if (!varValue.Equals(""))
                {
                    return varValue;
                }
            }

            if (rawVariable.StartsWith("ls["))
            {
                rawVariable = rawVariable.Replace("ls[", "[");
                varValue = ListRelativeElementChildrenByTag(currentElement, rawVariable);
                if (!varValue.Equals(""))
                {
                    return varValue;
                }
            }

            if (rawVariable.StartsWith("("))
            {
                varValue = ParseElementByKey(currentElement, rawVariable);
                if (!varValue.Equals(""))
                {
                    return varValue;
                }
            }

            if (rawVariable.StartsWith("["))
            {
                varValue = ParseRelativeElement(currentElement, rawVariable);
                if (!varValue.Equals(""))
                {
                    return varValue;
                }
            }

            varValue = ParseElementChildByTag(currentElement, rawVariable);
            if (!varValue.Equals(""))
            {
                return varValue;
            }

            return varValue;
        }

        private string ParseElementChildByTag(Classes.Element currentElement, string rawVariable)
        {
            var propValue = "";
            if (rawVariable.EndsWith("))"))
            {
                rawVariable = rawVariable += "output";
            }

            var arr = rawVariable.Split("))");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var key = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return "";
            }
            var elemChildren = curElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                }
                propValue += Tools.Elements.GetElementProperty(ch, prop);
            }

            return propValue;
        }

        private string ListElementChildrenByTag(Classes.Element currentElement, string rawVariable)
        {
            var propValue = "";
            if (rawVariable.EndsWith("))"))
            {
                rawVariable = rawVariable += "output";
            }

            var arr = rawVariable.Split("))");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var key = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return "";
            }
            var elemChildren = curElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                    propValue += Tools.Elements.GetElementProperty(ch, prop);
                }
            }

            return propValue;
        }

        private string ListRelativeElementChildrenByTag(Classes.Element currentElement, string rawVariable)
        {
            var propValue = "";
            if (rawVariable.EndsWith("))"))
            {
                rawVariable = rawVariable += "output";
            }

            var arr = rawVariable.Split("))");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var prop = arr[1].Trim();
            arr = arr[0].Split(")");
            if (arr.Length != 2)
            {
                return propValue;
            }

            var tag = arr[1].Replace("((", "").Trim();
            var relCode = arr[0].Replace("(", "").Trim();

            var elemDb = new DataManagement.GameLogic.Element();

            var relElement = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (relElement.ElementKey.Equals(""))
            {
                return "";
            }
            var elemChildren = relElement.Children.Where(c => c.Tags.TagsContain(tag));
            foreach (var ch in elemChildren)
            {
                if (!propValue.Equals(""))
                {
                    propValue += "|";
                    propValue += Tools.Elements.GetElementProperty(ch, prop);
                }
            }

            return propValue;
        }

        public string ParseElementByKey(Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.EndsWith(")"))
            {
                rawVariable = rawVariable += "output";
            }
            var arr = rawVariable.Split(")");
            if (arr.Length != 2)
            {
                return "";
            }

            var key = arr[0].Trim().Replace("(", "");
            var prop = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var curElement = elemDb.GetElementByKey(key);
            if (curElement.ElementKey.Equals(""))
            {
                return "";
            }

            return Tools.Elements.GetElementProperty(curElement, prop);
        }

        private string ParseRelativeElement(Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.EndsWith("]"))
            {
                rawVariable = rawVariable += "output";
            }

            var arr = rawVariable.Split("))");

            if (arr.Length == 2)
            {
                var outputValue = ParseRelativeElementByTag(currentElement, rawVariable);
                if (!outputValue.Equals(""))
                {
                    return outputValue;
                }
            }

            arr = rawVariable.Split("]");
            if (arr.Length != 2)
            {
                return "";
            }

            var relCode = $"{arr[0].Trim()}]";
            var prop = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var curElement = Tools.Elements.GetRelativeElement(currentElement, relCode);
            if (curElement.ElementKey.Equals(""))
            {
                return "";
            }

            return Tools.Elements.GetElementProperty(curElement, prop);
        }

        private string ParseRelativeElementByTag(Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.EndsWith(")"))
            {
                rawVariable = rawVariable += "output";
            }
            var arr = rawVariable.Split("))");
            var prop = "";
            if (arr.Length == 2)
            {
                prop = arr[1].Trim();
            }
            arr = arr[0].Split("]");
            if (arr.Length != 2)
            {
                return "";
            }
            var tag = arr[1].Trim().Replace("((", "");
            var rel = arr[0].Trim().Replace("[", "");
            rel = $"[{rel}]";

            var curElement = Tools.Elements.GetRelativeElement(currentElement, rel);
            var childElement = curElement.Children.Where(c => c.Tags.TagsContain(tag)).FirstOrDefault();

            if (childElement == null)
            {
                return "";
            }

            return Tools.Elements.GetElementProperty(childElement, prop);
        }

        private string ParsePresetValues(Classes.Element currentElement, string rawVariable)
        {
            var varValue = "";

            switch (rawVariable)
            {
                case "[isday]":
                    return Tools.Game.IsDay().ToString().ToLower();

                case "[missiondays]":
                    return Tools.Game.MissionDays().ToString().ToLower();

                case "[totaldays]":
                    return Tools.Game.TotalDays().ToString().ToLower();
            }

            return varValue;
        }

        public static bool DoComparison(string variable1, string variable2, string operand)
        {
            var success = false;
            string[] arr;

            if (variable1.Equals("") && variable2.Equals(""))
            {
                return true;
            }

            if (!operand.Equals("!=") && !operand.Equals("="))
            {
                if (variable1.Equals("") || variable2.Equals(""))
                {
                    return true;
                }
            }

            switch (operand)
            {
                case "!=":
                    if (variable1 != variable2)
                    {
                        success = true;
                    }
                    break;
                case ">":
                    try
                    {
                        if (Convert.ToInt32(variable1) > Convert.ToInt32(variable2))
                        {
                            success = true;
                        }
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case "<":
                    try
                    {
                        if (Convert.ToInt32(variable1) < Convert.ToInt32(variable2))
                        {
                            success = true;
                        }
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case ">=":
                    try
                    {
                        if (Convert.ToInt32(variable1) >= Convert.ToInt32(variable2))
                        {
                            success = true;
                        }
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case "<=":
                    try
                    {
                        if (Convert.ToInt32(variable1) <= Convert.ToInt32(variable2))
                        {
                            success = true;
                        }
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case "in":
                    if (variable2.ListContains(variable1))
                    {
                        success = true;
                    }
                    break;
                case "!in":
                    if (!variable2.ListContains(variable1))
                    {
                        success = true;
                    }
                    break;
                case "><":
                    arr = variable2.Split('|');
                    if (arr.Length == 2)
                    {
                        try
                        {
                            var chkVar = Convert.ToInt32(variable1);
                            var minChk = Convert.ToInt32(arr[0].Trim());
                            var maxChk = Convert.ToInt32(arr[1].Trim());
                            if (chkVar >= minChk && chkVar <= maxChk)
                            {
                                success = true;
                            }
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                    }
                    break;
                case "<>":
                    arr = variable2.Split('|');
                    if (arr.Length == 2)
                    {
                        try
                        {
                            var chkVar = Convert.ToInt32(variable1);
                            var minChk = Convert.ToInt32(arr[0].Trim());
                            var maxChk = Convert.ToInt32(arr[1].Trim());
                            if (chkVar < minChk || chkVar > maxChk)
                            {
                                success = true;
                            }
                        }
                        catch (Exception)
                        {
                            success = false;
                        }
                    }
                    break;
                default: // The default is =
                    if (variable1 == variable2)
                    {
                        success = true;
                    }
                    break;
            }

            return success;
        }
        #endregion


        public string ParseSetLogic(string logic, string tag)
        {
            var output = "";
            var msgParser = new Parsers.Elements.Message();

            output = msgParser.ProcessMessageText(logic, tag);

            return output;
        }
    }
}