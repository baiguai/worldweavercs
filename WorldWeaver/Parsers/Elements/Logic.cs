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

        private Classes.ConditionalVariable GetVariableValue(Classes.Element currentElement, string rawVariable)
        {
            if (rawVariable.StartsWith("?"))
            {
                rawVariable = rawVariable.Substring(1).Trim();
            }

            var variableOutput = new Classes.ConditionalVariable();
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
                        variableOutput.SubCondition = arrSubCond[1].Trim().ToLower();
                        break;
                    }
                }

                variableOutput.Condition = arrSubCond[0].Trim();
            }
            else
            {
                variableOutput.Condition = rawVariable;
            }

            if (variableOutput.Condition.StartsWith("'"))
            {
                variableOutput.Value = variableOutput.Condition.Replace("'", "");
                return variableOutput;
            }

            int test;
            var isnumeric = int.TryParse(variableOutput.Condition, out test);
            if (isnumeric)
            {
                variableOutput.Value = variableOutput.Condition;
                return variableOutput;
            }

            if (variableOutput.Condition.ToLower().Equals("[room]"))
            {
                variableOutput.Condition = Cache.RoomCache.Room.ElementKey;
            }

            if (variableOutput.Condition.ToLower().Equals("[self]"))
            {
                variableOutput.Condition = Tools.Elements.GetSelf(currentElement).ElementKey;
            }

            if (variableOutput.Condition.ToLower().Equals("[isday]"))
            {
                variableOutput.Value = Tools.Game.IsDay().ToString().ToLower();
                return variableOutput;
            }

            if (variableOutput.Condition.ToLower().Equals("[missiondays]"))
            {
                variableOutput.Value = Tools.Game.MissionDays().ToString().ToLower();
                return variableOutput;
            }

            if (variableOutput.Condition.ToLower().Equals("[totaldays]"))
            {
                variableOutput.Value = Tools.Game.TotalDays().ToString().ToLower();
                return variableOutput;
            }

            if (variableOutput.Condition.ToLower().Equals("[inventory]"))
            {
                variableOutput.Condition = "inventory";
            }

            foreach (var proc in variableProcs)
            {
                switch (proc)
                {
                    case "element_key":
                        varElement = elemLog.GetElementByKey(variableOutput.Condition);
                        if (varElement.ElementKey.Equals(""))
                        {
                            continue;
                        }
                        variableOutput.Type = "element";
                        variableOutput.Element = varElement;
                        switch (variableOutput.SubCondition)
                        {
                            case "location":
                                variableOutput.Value = variableOutput.Element.ParentKey;
                                return variableOutput;

                            case "tags":
                                variableOutput.Value = variableOutput.Element.Tags;
                                return variableOutput;

                            case "active":
                                variableOutput.Value = variableOutput.Element.Active;
                                return variableOutput;

                            case "type":
                                variableOutput.Value = variableOutput.Element.ElementType;
                                return variableOutput;
                        }
                        break;

                    case "element_attributes":
                        varElement = elemLog.GetElementByKey(variableOutput.Condition);
                        if (varElement.ElementKey.Equals(""))
                        {
                            continue;
                        }
                        variableOutput.Type = "attribute";
                        variableOutput.Element = varElement;
                        variableOutput.Value = varElement.Output;
                        return variableOutput;

                    case "inventory":
                        if (!variableOutput.Condition.Equals("inventory"))
                        {
                            continue;
                        }
                        var varCount = 0;
                        var invElems = elemLog.GetElementChildren("player");
                        foreach (var elem in invElems)
                        {
                            if (variableOutput.SubCondition.Equals("count") && elem.Tags.Contains("inventory"))
                            {
                                varCount++;
                            }
                        }
                        if (variableOutput.SubCondition.Equals("count"))
                        {
                            variableOutput.Value = varCount.ToString();
                            return variableOutput;
                        }
                        break;
                }
            }

            return variableOutput;
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


        public string ParseSetLogic(string logic, string tag)
        {
            var output = "";
            var msgParser = new Parsers.Elements.Message();

            output = msgParser.ProcessMessageText(logic, tag);

            return output;
        }
    }
}