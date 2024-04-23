using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Parsers.Elements;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class OutputParser
    {
        public string ParseOutput(string gameDb, string outputMessage) // @todo
        {
            var output = outputMessage;

            output = output.Replace("\\b", $"{Environment.NewLine}");
            output = ProcessSpecialFunctions(gameDb, output);
            output = ProcessFieldReplacement(gameDb, output);
            output = ProcessKeyReplacement(gameDb, output);
            output = ProcessInlineComparisons(output);

            return output;
        }

        private string ProcessSpecialFunctions(string gameDb, string output)
        {
            if (output.Contains("[isday]"))
            {
                output = output.Replace("[isday]", Tools.Game.IsDay(gameDb).ToString());
            }
            if (output.Contains("[time]"))
            {
                output = output.Replace("[time]", Tools.Game.CurrentTime(gameDb));
            }
            if (output.Contains("[totaldays]"))
            {
                output = output.Replace("[totaldays]", Tools.Game.TotalDays(gameDb).ToString("N0"));
            }
            if (output.Contains("[missiondays]"))
            {
                output = output.Replace("[missiondays]", Tools.Game.TotalDays(gameDb).ToString("N0"));
            }
            if (output.Contains("[player_armed_weapon]"))
            {
                try
                {
                    var weaponKey = Cache.PlayerCache.Player.AttributeByTag("armed").Output;
                    var weaponElem = Cache.PlayerCache.Player.ChildByKey(weaponKey);
                    output = output.Replace("[player_armed_weapon]", weaponElem.ChildByTag("title").Output);
                }
                catch (Exception)
                {
                    output = output.Replace("[player_armed_weapon]", "");
                }
            }

            return output;
        }

        private string ProcessFieldReplacement(string gameDb, string outputText)
        {
            var output = outputText;
            var fldStart = outputText.IndexOf("[field");
            var errMsg = $"Error: [field] logic is malformed in:{Environment.NewLine + Environment.NewLine}{output}";

            while (fldStart > -1)
            {
                var fldEnd = output.IndexOf(']', fldStart) + 1;

                if (fldEnd <= fldStart)
                {
                    return errMsg;
                }

                var fld = output[fldStart..fldEnd];
                var arr = fld.Replace("[", "").Replace("]", "").Split('|');
                var key = "";
                var field = "";
                var newValue = "";
                var elemDb = new DataManagement.GameLogic.Element();

                if (arr.Length == 3)
                {
                    key = arr[1].Trim();
                    field = arr[2].Trim();
                    newValue = elemDb.GetElementField(gameDb, key, field);
                    output = output.Replace(fld, newValue);
                }
                else
                {
                    return errMsg;
                }

                fldStart = output.IndexOf("[field");
            }

            return output;
        }

        private string ProcessKeyReplacement(string gameDb, string outputText)
        {
            var output = outputText;
            var keyStart = outputText.IndexOf("[key");
            var errMsg = $"Error: [field] logic is malformed in:{Environment.NewLine + Environment.NewLine}{output}";

            while (keyStart > -1)
            {
                var keyEnd = output.IndexOf(']', keyStart) + 1;

                if (keyEnd <= keyStart)
                {
                    return errMsg;
                }

                var keyRef = output[keyStart..keyEnd];
                var arr = keyRef.Replace("[", "").Replace("]", "").Split('|');
                var key = "";
                var field = "";
                var newValue = "";
                var elemDb = new DataManagement.GameLogic.Element();

                if (arr.Length == 2)
                {
                    key = arr[1].Trim();
                    var elem = elemDb.GetElementByKey(gameDb, key);
                    var elemParser = new Parsers.Elements.Element();
                    if (elem.ElementType.Equals(""))
                    {
                        elem.ElementType = "general";
                    }
                    var procItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                    var outputObj = new Classes.Output();
                    foreach (var proc in procItems)
                    {
                        outputObj = elemParser.ParseElement(outputObj, gameDb, elem, "", proc);
                    }
                    output = output.Replace(keyRef, outputObj.OutputText);
                }
                else
                {
                    return errMsg;
                }

                keyStart = output.IndexOf("[field");
            }

            return output;
        }


        private string ProcessInlineComparisons(string outputText)
        {
            var output = outputText;
            var lgcStart = output.IndexOf("[?");
            var errMsg = $"Error: [? ... ?] logic is malformed in:{Environment.NewLine + Environment.NewLine}{output}";

            while (lgcStart > -1)
            {
                var lgcEnd = output.IndexOf("?]", lgcStart) + 2;

                if (lgcEnd <= lgcStart)
                {
                    return errMsg;
                }

                var lgcBlockToReplace = output[lgcStart..lgcEnd];
                var lgcBlock = lgcBlockToReplace.Replace("[?", "").Replace("?]", "");
                var ansStart = lgcBlock.IndexOf("[=");
                var answers = new List<string>();

                while (ansStart > -1)
                {
                    var ansEnd = lgcBlock.IndexOf("=]", ansStart) + 2;

                    if (ansEnd <= ansStart)
                    {
                        return errMsg;
                    }

                    var ans = lgcBlock[ansStart..ansEnd];
                    answers.Add(ans.Replace("[=", "").Replace("=]", ""));
                    lgcBlock = lgcBlock.Replace(ans, "");

                    ansStart = lgcBlock.IndexOf("[=");
                }

                if (answers.Count != 2)
                {
                    return errMsg;
                }

                lgcBlock = lgcBlock.Trim();
                var operand = Logic.GetOperand(lgcBlock);
                var arr = lgcBlock.Split(operand);

                if (arr.Length != 2)
                {
                    return errMsg;
                }

                if (Logic.DoComparison(arr[0].Trim(), arr[1].Trim(), operand))
                {
                    lgcBlock = answers[0];
                }
                else
                {
                    lgcBlock = answers[1];
                }

                output = output.Replace(lgcBlockToReplace, lgcBlock);

                lgcStart = output.IndexOf("[?");
            }

            return output;
        }
    }
}