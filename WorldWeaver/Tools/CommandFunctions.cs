using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace WorldWeaver.Tools
{
    public static class CommandFunctions
    {
        public static string GetCommandMethod(string input, string parser)
        {
            var output = "";

            using (StreamReader r = new StreamReader($"Config/Commands/{parser}.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var cmd in jsonObj["commands"])
                {
                    var syntax = (string)cmd["pattern"];
                    var method = (string)cmd["method"];

                    Regex rgx = new Regex(syntax, RegexOptions.IgnoreCase);

                    if (rgx.IsMatch(input))
                    {
                        output = method;
                    }
                }
            }

            return output;
        }

        public static string GetHelpTopic(string input, string system)
        {
            var output = "";

            foreach (var f in Directory.GetFiles($"Help/{system}"))
            {
                using (StreamReader r = new StreamReader(f))
                {
                    string json = r.ReadToEnd();
                    var jsonObj = JObject.Parse(json);
                    foreach (var cmd in jsonObj["topics"])
                    {
                        var syntax = (string)cmd["pattern"];
                        var title = (string)cmd["title"];
                        var content = (string)cmd["string"];

                        Regex rgx = new Regex(syntax, RegexOptions.IgnoreCase);

                        if (rgx.IsMatch(input))
                        {
                            if (!output.Equals(""))
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";
                            }

                            output += $"{title}{Environment.NewLine}";
                            output += $"--------------------------------------------------------------------------------{Environment.NewLine}";
                            output += content;
                        }
                    }
                }
            }

            return output;
        }
    }
}
