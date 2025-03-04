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
    public class Test
    {
        public static void ParseTest(Classes.Element currentElement)
        {
            if (!MainClass.macro.IsRunning || !MainClass.macro.DoTests)
            {
                return;
            }

            var currentOutput = MainClass.output.OutputText;
            var currentFailedLgc = MainClass.output.FailedLogic;
            var elemDb = new DataManagement.GameLogic.Element();
            var curElem = elemDb.GetElementByKey(currentElement.ElementKey);

            if (!curElem.Tags.TagsContain("!_test"))
            {
                return;
            }

            var lgcParser = new Parsers.Elements.Logic();
            lgcParser.ParseLogic(currentElement);

            if (!MainClass.output.FailedLogic)
            {
                var testRes = $"Test - {currentElement.Name} result: PASS";
                if (!MainClass.testResults.Contains(testRes))
                {
                    MainClass.testResults.Add(testRes);
                }
            }
            else
            {
                var failedRes = $"Test - {currentElement.Name} result: FAIL";
                if (!MainClass.testResults.Contains(failedRes))
                {
                    MainClass.testResults.Add(failedRes);
                }
            }

            MainClass.output.OutputText = currentOutput;
            MainClass.output.FailedLogic = currentFailedLgc;

            return;
        }
    }
}
