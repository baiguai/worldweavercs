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
            var helpOutput = "";

            helpOutput = ProcessHelpDirectory($"Help/{system}", input);

            if (helpOutput.Equals(""))
            {
                helpOutput = SearchHelp(input, system);
            }

            return helpOutput;
        }

        private static string ProcessHelpDirectory(string helpDir, string input)
        {
            var helpOutput = "";

            foreach (var d in Directory.GetDirectories(helpDir))
            {
                helpOutput = ProcessHelpDirectory(d, input);
                if (!helpOutput.Equals(""))
                {
                    return helpOutput;
                }
            }

            foreach (var f in Directory.GetFiles(helpDir))
            {
                if (!f.Contains(".json"))
                {
                    continue;
                }

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
                            if (!helpOutput.Equals(""))
                            {
                                helpOutput += $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";
                            }

                            helpOutput += $"{title}{Environment.NewLine}";
                            helpOutput += $"--------------------------------------------------------------------------------{Environment.NewLine}";
                            helpOutput += content;

                            if (links.Count > 0)
                            {
                                helpOutput += $"{Environment.NewLine}{Environment.NewLine}";
                                helpOutput += $"Links{Environment.NewLine}";
                                helpOutput += $"-----{Environment.NewLine}";
                                foreach (var l in links)
                                {
                                    helpOutput += $"{l}{Environment.NewLine}";
                                }
                            }

                            return helpOutput;
                        }
                    }
                }
            }

            return helpOutput;
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
            var pfx = "help ";
            if (system.Equals("Admin"))
            {
                pfx = "_help ";
            }

            return ProcessHelpSearchDirectory($"Help/{system}", pfx, input, "");
        }

        private static string ProcessHelpSearchDirectory(string helpDir, string pfx, string input, string helpSrchOutput)
        {
            var rgxString = "";
            var matchedStr = "";


            rgxString = @$"(?i)\b({input.Replace(' ', '.')})\b";

            Regex rgx = new Regex(rgxString, RegexOptions.IgnoreCase);

            foreach (var d in Directory.GetDirectories(helpDir))
            {
                helpSrchOutput = ProcessHelpSearchDirectory(d, pfx, input, helpSrchOutput);
            }

            foreach (var f in Directory.GetFiles(helpDir))
            {
                if (!f.Contains(".json"))
                {
                    continue;
                }

                using (StreamReader r = new StreamReader(f))
                {
                    string json = r.ReadToEnd();
                    var jsonObj = JObject.Parse(json);
                    foreach (var cmd in jsonObj["topics"])
                    {
                        var syntax = (string)cmd["pattern"];
                        var title = (string)cmd["title"];
                        var content = (string)cmd["string"];

                        if (rgx.IsMatch(syntax) || title.Contains(input, StringComparison.CurrentCultureIgnoreCase) || content.Contains(input, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!helpSrchOutput.Equals(""))
                            {
                                helpSrchOutput += $"{Environment.NewLine}";
                            }

                            matchedStr = syntax.Replace("\\b(", "").Replace(")\\b", "").Replace("?:", "").Replace(".", " ");
                            if (matchedStr.Trim() + " " != pfx)
                            {
                                helpSrchOutput += $"{pfx}{matchedStr}";
                            }
                        }
                    }
                }
            }

            return helpSrchOutput;
        }
    }
}
