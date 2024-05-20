using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class OutputProcessor
    {
        public static string ProcessSpecialValues(string output, Classes.Element currentElement)
        {
            var newOutput = "";
            var startPos = 0;
            var endPos = 0;
            var specialString = "";
            var specialStringReplc = "";
            var newValue = "";

            startPos = output.IndexOf("<<");

            while (startPos >= 0)
            {
                endPos = output.IndexOf(">>", startPos);
                specialString = output.Substring(startPos, endPos);
                newValue = GetNewValue(currentElement, specialString);
                output = output.Replace($"<<{specialString}>>", $"<<{newValue}>>");
                startPos = output.IndexOf("<<");
            }

            return newOutput;
        }

        private static string GetNewValue(Element currentElement, string specialString)
        {
            var updated = specialString;

            if (specialString.Equals("isday")
            {
                updated = Tools.Game.IsDay().ToString().ToLower();
                return updated;
            }

            if (specialString.Equals("missiondays")
            {
                updated = Tools.Game.MissionDays().ToString().ToLower();
                return updated;
            }

            if (specialString.Equals("totaldays")
            {
                updated = Tools.Game.TotalDays().ToString().ToLower();
                return updated;
            }

            return updated;
        }
    }
}