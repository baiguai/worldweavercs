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
            MainClass.output.MatchMade = true;
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
                        MainClass.output.OutputText = currentElement.Output;
                        return;
                    }
                    else
                    {
                        MainClass.output.FailedLogic = false;
                        MainClass.output.OutputText += currentElement.Output;
                        return;
                    }
                }
            }

            MainClass.output.FailedLogic = !passed;

            return;
        }

        private bool ParseConditional(Classes.Element currentElement, string[] lines)
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

            return varValue;
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
                    propValue += GetElementProperty(ch, prop);
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

            var relElement = GetRelativeElement(currentElement, relCode);
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
                    propValue += GetElementProperty(ch, prop);
                }
            }

            return propValue;
        }

        private string ParseElementByKey(Classes.Element currentElement, string rawVariable)
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

            return GetElementProperty(curElement, prop);
        }

        private string ParseRelativeElement(Classes.Element currentElement, string rawVariable)
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

            var relCode = arr[0].Trim().Replace("(", "");
            var prop = arr[1].Trim();

            var elemDb = new DataManagement.GameLogic.Element();
            var curElement = GetRelativeElement(currentElement, relCode);
            if (curElement.ElementKey.Equals(""))
            {
                return "";
            }

            return GetElementProperty(curElement, prop);
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

        private string GetElementProperty(Classes.Element curElement, string elementProperty)
        {
            switch (elementProperty.ToLower())
            {
                case "elementkey":
                    return curElement.ElementKey;

                case "elementtype":
                    return curElement.ElementType;

                case "parentkey":
                    return curElement.ParentKey;

                case "syntax":
                    return curElement.Syntax;

                case "logic":
                    return curElement.Logic;

                case "tags":
                    return curElement.Tags;

                case "active":
                    return curElement.Active;

                default:
                    return curElement.Output;
            }
        }

        private Classes.Element GetRelativeElement(Classes.Element currentElement, string relCode)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            switch (relCode.ToLower())
            {
                case "[self]":
                    return Tools.Elements.GetSelf(currentElement);

                case "[enemy]":
                    if (Cache.FightCache.Fight == null)
                    {
                        return new Classes.Element();
                    }
                    return Cache.FightCache.Fight.Enemy;

                default:
                    return Cache.RoomCache.Room;
            }
        }

        public static bool DoComparison(string variable1, string variable2, string operand)
        {
            var success = false;
            string[] arr;

            if (variable1.Equals("") && variable2.Equals(""))
            {
                return true;
            }

            if (!operand.Equals("!=") && !operand.Equals("=="))
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