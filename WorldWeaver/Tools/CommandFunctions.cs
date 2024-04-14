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
                    var links = new List<string>();
                    foreach (var cmd in jsonObj["topics"])
                    {
                        var syntax = (string)cmd["pattern"];
                        var title = (string)cmd["title"];
                        var content = (string)cmd["string"];

                        content = content.Replace("---", "--------------------------------------------------------------------------------");

                        if (cmd["links"] != null) 
                        {
                            foreach (var link in cmd["links"])
                            {
                                links.Add((string)link);
                            }
                        }

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

                            if (links.Count > 0)
                            {
                                output += $"{Environment.NewLine}{Environment.NewLine}";
                                output += $"Links{Environment.NewLine}";
                                output += $"-----{Environment.NewLine}";
                                foreach (var l in links)
                                {
                                    output += $"{l}{Environment.NewLine}";
                                }
                            }
                        }
                    }
                }
            }

            if (output.Equals(""))
            {
                output = SearchHelp(input, system);
            }

            return output;
        }

        internal static bool GetDuringGameOption(string input, string parser)
        {
            var output = false;

            using (StreamReader r = new StreamReader($"Config/Commands/{parser}.json"))
            {
                string json = r.ReadToEnd();
                var jsonObj = JObject.Parse(json);
                foreach (var cmd in jsonObj["commands"])
                {
                    var syntax = (string)cmd["pattern"];
                    var duringGame = (string)cmd["during_game"];

                    Regex rgx = new Regex(syntax, RegexOptions.IgnoreCase);

                    if (rgx.IsMatch(input))
                    {
                        output = duringGame != "false";
                    }
                }
            }

            return output;
        }

        private static string SearchHelp(string input, string system)
        {
            var output = "";
            var rgxString = "";
            var pfx = "help ";
            var matchedStr = "";

            if (system.Equals("Admin"))
            {
                pfx = "_help ";
            }

            rgxString = @$"(?i)\b({input})\b";

            Regex rgx = new Regex(rgxString, RegexOptions.IgnoreCase);

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

                        if (rgx.IsMatch(syntax) || rgx.IsMatch(title) || rgx.IsMatch(content))
                        {
                            if (!output.Equals(""))
                            {
                                output += $"{Environment.NewLine}";
                            }

                            matchedStr = syntax.Replace("\\b(", "").Replace(")\\b", "");
                            if (matchedStr.Trim()+" " != pfx)
                            {
                                output += $"{pfx}{matchedStr}";
                            }
                        }
                    }
                }
            }

            return output;
        }
    }
}
