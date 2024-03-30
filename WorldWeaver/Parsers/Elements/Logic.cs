using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var logicType = "";

            foreach (var line in lines)
            {
                if (logicType.Equals("comparison"))
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
                }

                if (line.StartsWith("?"))
                {
                    logicType = "comparison";
                    operand = GetOperand(line);
                    var arr = line.Split(operand);
                    if (arr.Length == 2)
                    {
                        var variable1 = GetVariableValue(gameDb, currentElement, arr[0].Trim());
                        var variable2 = GetVariableValue(gameDb, currentElement, arr[1].Trim());
                        curCheck = DoComparison(variable1, variable2, operand);
                    }
                }
            }

            return passed;
        }

        #region Conditional Helpers
        private string GetOperand(string line)
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
            var output = new Classes.ConditionalVariable();
            var variableProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicVariableTypes.json");
            var subCondProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicSubConditions.json");
            var elemLog = new DataManagement.GameLogic.Element();
            var arrSubCond = rawVariable.Split(".");
            var value = "";
            
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

                value = arrSubCond[0].Trim();
            }
            else
            {
                value = rawVariable;
            }

            if (output.Value.StartsWith('"'))
            {
                output.Value = output.Value.Replace("\"", "");
                return output;
            }

            int test;
            var isnumeric = int.TryParse(output.Value, out test);
            if (isnumeric)
            {
                return output;
            }

            if (rawVariable.ToLower().Equals("[room]"))
            {
                value = Cache.RoomCache.Room.ElementKey;
            }

            if (rawVariable.ToLower().Equals("[self]"))
            {
                value = Tools.Elements.GetSelf(gameDb, currentElement).ElementKey;
            }

            foreach (var proc in variableProcs)
            {
                switch (proc)
                {
                    case "element_key":
                        varElement =elemLog.GetElementByKey(gameDb, value);
                        output.Type = "element";
                        output.Element = varElement;
                        switch (output.SubCondition)
                        {
                            case "location":
                                output.Value = output.Element.ParentKey;
                                break;

                            case "tags":
                                output.Value = output.Element.Tags;
                                break;

                            case "active":
                                output.Value = output.Element.Active;
                                break;

                            case "type":
                                output.Value = output.Element.ElementType;
                                break;
                        }
                        break;

                    case "element_attributes":
                        varElement = elemLog.GetElementByKey(gameDb, value);
                        output.Type = "attribute";
                        output.Element = varElement;
                        output.Value = varElement.Output;
                        break;

                    case "inventory":
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

        private bool DoComparison(ConditionalVariable variable1, ConditionalVariable variable2, string operand)
        {
            var success = false;
            string[] arr;

            switch (operand)
            {
                case "!=":
                    if (variable1.Value != variable2.Value)
                    {
                        success = true;
                    }
                    break;
                case ">":
                    try
                    {
                        if (Convert.ToInt32(variable1.Value) > Convert.ToInt32(variable2.Value))
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
                        if (Convert.ToInt32(variable1.Value) < Convert.ToInt32(variable2.Value))
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
                        if (Convert.ToInt32(variable1.Value) >= Convert.ToInt32(variable2.Value))
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
                        if (Convert.ToInt32(variable1.Value) <= Convert.ToInt32(variable2.Value))
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
                    if (variable2.Value.Contains(variable1.Value))
                    {
                        success = true;
                    }
                    break;
                case "!in":
                    if (!variable2.Value.Contains(variable1.Value))
                    {
                        success = true;
                    }
                    break;
                case "><":
                    arr = variable2.Value.Split('|');
                    if (arr.Length == 2)
                    {
                        try
                        {
                            var chkVar = Convert.ToInt32(variable1.Value);
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
                    arr = variable2.Value.Split('|');
                    if (arr.Length == 2)
                    {
                        try
                        {
                            var chkVar = Convert.ToInt32(variable1.Value);
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
                    if (variable1.Value == variable2.Value)
                    {
                        success = true;
                    }
                    break;
            }

            return success;
        }
#endregion


        public string ParseSetLogic(string gameDb, string logic, string userInput)
        {
            var output = "";

            output = logic.Replace("[input]", userInput);

            return output;
        }
    }
}