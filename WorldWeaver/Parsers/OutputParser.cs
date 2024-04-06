using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Parsers.Elements;

namespace WorldWeaver.Parsers
{
    public class OutputParser
    {
        public string ParseOutput(string gameDb, string outputMessage)
        {
            var output = outputMessage;

            output = output.Replace("\\b", $"{Environment.NewLine}");
            output = ProcessFieldReplacement(gameDb, output);
            output = ProcessInlineComparisons(output);

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