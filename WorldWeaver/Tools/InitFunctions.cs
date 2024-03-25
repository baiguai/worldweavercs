using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace WorldWeaver.Tools
{
    public class InitFunctions
    {
        public static string GetInitMessage(bool IsLaunching = true)
        {
            var output = "";

            using (StreamReader r = new StreamReader($"Config/Init.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var msg in jsonObj["message"])
                {
                    output = (string)msg["output"];
                    break;
                }
            }

            if (IsLaunching)
            {
                output += $"{Environment.NewLine}{Environment.NewLine}>>";
            }

            return output;
        }
    }
}
