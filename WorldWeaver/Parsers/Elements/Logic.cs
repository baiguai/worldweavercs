using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLitePCL;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Logic
    {
        public Classes.Output ParseLogic(Classes.Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            output.MatchMade = true;
            var passed = true;
            var lines = currentElement.Logic.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            );
            
            foreach (var line in lines)
            {
                if (line.StartsWith("?"))
                {
                    passed = ParseConditional(gameDb, currentElement, lines);
                    if (!passed)
                    {
                        output.FailedLogic = true;
                        output.OutputText = currentElement.Output;
                        return output;
                    }
                    else
                    {
                        output.FailedLogic = false;
                        output.OutputText += currentElement.Output;
                        return output;
                    }
                }
            }

            output.FailedLogic = !passed;

            return output;
        }

        private bool ParseConditional(string gameDb, Classes.Element currentElement, string[] lines)
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
                        var variable1 = GetVariableValue(gameDb, currentElement, arr[0].Trim());
                        var variable2 = GetVariableValue(gameDb, currentElement, arr[1].Trim());
                        curCheck = DoComparison(variable1.Value, variable2.Value, operand);
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

        private Classes.ConditionalVariable GetVariableValue(string gameDb, Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.StartsWith("?"))
            {
                rawVariable = rawVariable.Substring(1).Trim();
            }

            var output = new Classes.ConditionalVariable();
            var variableProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicVariableTypes.json");
            var subCondProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicSubConditions.json");
            var elemLog = new DataManagement.GameLogic.Element();
            var arrSubCond = rawVariable.Split(".");
            
            Classes.Element varElement = null;

            if (arrSubCond.Length == 2)
            {
                foreach (var proc in subCondProcs)
                {
                    if (arrSubCond[1].Trim().ToLower().Equals(proc.ToLower()))
                    {
                        output.SubCondition = arrSubCond[1].Trim().ToLower();
                        break;
                    }
                }

                output.Condition = arrSubCond[0].Trim();
            }
            else
            {
                output.Condition = rawVariable;
            }

            if (output.Condition.StartsWith("'"))
            {
                output.Value = output.Condition.Replace("'", "");
                return output;
            }

            int test;
            var isnumeric = int.TryParse(output.Condition, out test);
            if (isnumeric)
            {
                output.Value = output.Condition;
                return output;
            }

            if (output.Condition.ToLower().Equals("[room]"))
            {
                output.Condition = Cache.RoomCache.Room.ElementKey;
            }

            if (output.Condition.ToLower().Equals("[self]"))
            {
                output.Condition = Tools.Elements.GetSelf(gameDb, currentElement).ElementKey;
            }

            if (output.Condition.ToLower().Equals("[isday]"))
            {
                output.Value = Tools.Game.IsDay(gameDb).ToString().ToLower();
                return output;
            }

            if (output.Condition.ToLower().Equals("[missiondays]"))
            {
                output.Value = Tools.Game.MissionDays(gameDb).ToString().ToLower();
                return output;
            }

            if (output.Condition.ToLower().Equals("[totaldays]"))
            {
                output.Value = Tools.Game.TotalDays(gameDb).ToString().ToLower();
                return output;
            }

            if (output.Condition.ToLower().Equals("[inventory]"))
            {
                output.Condition = "inventory";
            }

            foreach (var proc in variableProcs)
            {
                switch (proc)
                {
                    case "element_key":
                        varElement = elemLog.GetElementByKey(gameDb, output.Condition);
                        if (varElement.ElementKey.Equals(""))
                        {
                            continue;
                        }
                        output.Type = "element";
                        output.Element = varElement;
                        switch (output.SubCondition)
                        {
                            case "location":
                                output.Value = output.Element.ParentKey;
                                return output;

                            case "tags":
                                output.Value = output.Element.Tags;
                                return output;

                            case "active":
                                output.Value = output.Element.Active;
                                return output;

                            case "type":
                                output.Value = output.Element.ElementType;
                                return output;
                        }
                        break;

                    case "element_attributes":
                        varElement = elemLog.GetElementByKey(gameDb, output.Condition);
                        if (varElement.ElementKey.Equals(""))
                        {
                            continue;
                        }
                        output.Type = "attribute";
                        output.Element = varElement;
                        output.Value = varElement.Output;
                        return output;

                    case "inventory":
                        if (!output.Condition.Equals("inventory"))
                        {
                            continue;
                        }
                        var varCount = 0;
                        var invElems = elemLog.GetElementChildren(gameDb, "player");
                        foreach (var elem in invElems)
                        {
                            if (output.SubCondition.Equals("count") && elem.Tags.Contains("inventory"))
                            {
                                varCount++;
                            }
                        }
                        if (output.SubCondition.Equals("count"))
                        {
                            output.Value = varCount.ToString();
                            return output;
                        }
                        break;
                }
            }

            return output;
        }

        public static bool DoComparison(string variable1, string variable2, string operand)
        {
            var success = false;
            string[] arr;

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
                    if (variable2.Contains(variable1))
                    {
                        success = true;
                    }
                    break;
                case "!in":
                    if (!variable2.Contains(variable1))
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


        public string ParseSetLogic(string gameDb, string logic, string userInput, string tag)
        {
            var output = "";
            var msgParser = new Parsers.Elements.Message();

            output = msgParser.ProcessMessageText(logic, userInput, tag);

            return output;
        }
    }
}