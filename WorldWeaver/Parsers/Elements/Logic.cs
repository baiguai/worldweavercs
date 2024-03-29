using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    passed = ParseConditional(gameDb, lines);
                    break;
                }
            }

            output.FailedLogic = !passed;

            return output;
        }

        private bool ParseConditional(string gameDb, string[] lines)
        {
            var passed = true;
            var curCheck = true;
            var operand = "";
            var isAnd = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("?"))
                {
                    operand = GetOperand(line);
                    var arr = line.Split(operand);
                    if (arr.Length == 2)
                    {
                        var variable1 = GetVariableValue(gameDb, arr[0].Trim());
                        var variable2 = GetVariableValue(gameDb, arr[1].Trim());
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

        private Classes.ConditionalVariable GetVariableValue(string gameDb, string rawVariable)
        {
            var output = new Classes.ConditionalVariable();
            var variableProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicVariableTypes.json");
            var subCondProcs = Tools.AppSettingFunctions.GetRootArray("Config/LogicSubConditions.json");
            var elemLog = new DataManagement.GameLogic.Element();
            var arrSubCond = rawVariable.Split(".");
            var varCount = 0;
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

                output.Value = arrSubCond[0].Trim();
            }
            else
            {
                output.Value = rawVariable;
            }

            if (output.Value.StartsWith('"'))
            {
                output.Value = output.Value.Replace("\"", "");
                return output;
            }

            foreach (var proc in variableProcs)
            {
                switch (proc)
                {
                    case "element_key":
                        varElement =elemLog.GetElementByKey(gameDb, output.Value);
                        output.Type = "element";
                        output.Element = varElement;
                        switch (output.SubCondition)
                        {
                            case "location":
                                output.Value = output.Element.ParentKey;
                                break;
                        }
                        break;

                    case "element_attributes":
                        varElement = elemLog.GetElementByKey(gameDb, output.Value);
                        output.Type = "attribute";
                        output.Element = varElement;
                        output.Value = varElement.Output;
                        break;

                    case "inventory":
                        varCount = 0;
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
#endregion


        public string ParseSetLogic(string gameDb, string logic, string userInput)
        {
            var output = "";

            output = logic.Replace("[input]", userInput);

            return output;
        }
    }
}