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

            var logic = new DataManagement.GameLogic.Element();
            var logicParser = new Parsers.Elements.Logic();
            var currentOutput = MainClass.output.OutputText;
            var currentFailedLgc = MainClass.output.FailedLogic;
            var testElems = new List<Classes.Element>();
            var elemDb = new DataManagement.GameLogic.Element();
            var curElem = elemDb.GetElementByKey(currentElement.ElementKey);
            var children = curElem.Children.Where(c => c.Tags.TagsContain("!_test"));

            foreach (var c in children)
            {
                testElems.Add(c);
            }

            foreach (var testElem in testElems)
            {
                var testChildren = logic.GetElementChildren(testElem.ElementKey, false);
                foreach (var tst in testChildren.Where(t => t.ElementType.Equals("logic", StringComparison.OrdinalIgnoreCase)))
                {
                    logicParser.ParseLogic(tst);

                    if (!MainClass.output.FailedLogic)
                    {
                        var testRes = $"Test - {tst.Name} result: PASS";
                        if (!MainClass.testResults.Contains(testRes))
                        {
                            MainClass.testResults.Add(testRes);
                        }
                    }
                    else
                    {
                        var failedRes = $"Test - {tst.Name} result: FAIL";
                        if (!MainClass.testResults.Contains(failedRes))
                        {
                            MainClass.testResults.Add(failedRes);
                        }
                    }
                }

                MainClass.output.OutputText = currentOutput;
                MainClass.output.FailedLogic = currentFailedLgc;
            }

            return;
        }
    }
}
