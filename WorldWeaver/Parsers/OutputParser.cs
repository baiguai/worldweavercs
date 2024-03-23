using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Parsers
{
    public class OutputParser
    {
        public string ParseOutput(string gameDb, string outputMessage)
        {
            var output = outputMessage;

            while (output.IndexOf("[field") >= 0)
            {
                output = ProcessFieldReplacement(gameDb, output);
            }

            return output;
        }

        private string ProcessFieldReplacement(string gameDb, string outputText)
        {
            var output = outputText;
            var fldStart = outputText.IndexOf("[field");
            var fldEnd = outputText.IndexOf(']', fldStart) + 1;
            var fld = outputText[fldStart..fldEnd];
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
                return output;
            }
            else
            {
                return output.Replace(fld, "");
            }

            return output;
        }
    }
}