using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.Game
{
    public class ValidateGame
    {
        public string report = "";
        public int depth = 0;

        public List<string> elementKeys = new List<string>();

        public void Validate(string game_key)
        {
            var gameFile = $"./Games/{game_key.FileSafe()}.db";
            var gameDirString = game_key.FileSafe();
            var gameDirectory = $"./GameSources/{gameDirString}/";
            var heading = $@"REPORT:
--------------------------------------------------------------------------------

";

            ValidateGameFiles(gameFile, gameDirectory);

            MainClass.output.MatchMade = true;

            if (!report.Equals(""))
            {
                MainClass.output.OutputText = $"{heading}{report}";
            }
            else
            {
                MainClass.output.OutputText = "";
            }
        }

        private void ValidateGameFiles(string gameFile, string gameDirectory)
        {
            var connectionString = $"Data Source={gameFile};Cache=Shared;";

            foreach (var dir in Directory.GetDirectories(gameDirectory))
            {
                ValidateGameFiles(gameFile, dir);
            }

            foreach (string file in Directory.GetFiles(gameDirectory))
            {
                if (Directory.Exists(file))
                {
                    ValidateGameFiles(gameFile, file);
                }
                else if (Path.GetExtension(file).Equals(".nrmn"))
                {
                    var lines = File.ReadAllLines(file).ToList();
                    ValidateElementKeys(lines, "root", 0);
                }
            }
        }

        private int ValidateElementKeys(List<string> lines, string parentKey, int startLine)
        {
            var bldGame = new DataManagement.Game.BuildGame();
            var success = false;
            var currentDepth = depth;
            Classes.Element element = null;
            var initRow = startLine;
            var currentRow = startLine;

            for (int ix = startLine; ix < lines.Count; ix++)
            {
                currentRow = ix;
                var line = lines[ix].Trim();

                if (!bldGame.ParsableRow(line))
                {
                    continue;
                }

                if (depth > currentDepth && !line.Equals("}"))
                {
                    continue;
                }

                if (line.StartsWith("{", StringComparison.OrdinalIgnoreCase))
                {
                    var singleLine = line.EndsWith("}", StringComparison.OrdinalIgnoreCase);

                    if (singleLine)
                    {
                        line = line.Substring(0, line.Length - 1).Trim();
                    }

                    if (element == null)
                    {
                        element = new Classes.Element
                        {
                            ParentKey = parentKey,
                            Sort = ix
                        };

                        element = ParseInlineProperties(element, line);

                        MainClass.logger.WriteToLog($"New Element created: {element.ElementKey}, Parent: {element.ParentKey}", Logger.LogTypes.BuildGame);

                        if (line.Length > 1)
                        {
                            if (element.ParentKey == null)
                            {
                                element.ParentKey = parentKey;
                            }
                        }

                        if (singleLine)
                        {
                            if (element.ElementKey == "" || element.ElementKey == null)
                            {
                                element.ElementKey = Guid.NewGuid().ToString();
                            }

                            TryAddKey(element);
                            return currentRow;
                        }

                        continue;
                    }
                    else
                    {
                        depth++;
                        if (element.ElementKey == null || element.ElementKey.Equals(""))
                        {
                            element.ElementKey = Guid.NewGuid().ToString();
                        }
                        ix = ValidateElementKeys(lines, element.ElementKey, ix);
                        depth--;

                        // This should never happen
                        if (depth < 0)
                        {
                            depth = 0;
                        }

                        if (ix < 0)
                        {
                            return -1;
                        }

                        continue;
                    }
                }

                switch (line)
                {
                    case string s when line.ToLower().StartsWith("key=", StringComparison.OrdinalIgnoreCase):
                        element.ElementKey = line.Replace("key=", "").Replace(' ', '_').SqlSafe();
                        break;

                    case "}":
                        if (element == null)
                        {
                            MainClass.logger.WriteToLog($"Returning row-index: {currentRow - 1}", Logger.LogTypes.BuildGame);
                            return currentRow - 1;
                        }

                        if (depth == currentDepth)
                        {
                            if (element.ElementKey == null || element.ElementKey == "")
                            {
                                element.ElementKey = Guid.NewGuid().ToString();
                            }

                            TryAddKey(element);
                            element = null;
                        }
                        break;
                }
            }

            if (element != null) // @place
            {
                TryAddKey(element);
            }

            return currentRow;
        }

        private Classes.Element ParseInlineProperties(Classes.Element element, string dataRow)
        {
            dataRow = dataRow.Replace("{", "");
            var arr = dataRow.Split(',');

            foreach (var pair in arr)
            {
                var pairArr = pair.Trim().Split('=');
                if (pairArr.Length == 2)
                {
                    switch (pairArr[0].Trim().ToLower())
                    {
                        case "key":
                            element.ElementKey = pairArr[1].Trim().Replace(' ', '_');
                            break;
                    }
                }
            }

            return element;
        }

        private void TryAddKey(Element element)
        {
            if (!elementKeys.Contains(element.ElementKey))
            {
                elementKeys.Add(element.ElementKey);
            }
            else
            {
                if (!report.Equals(""))
                {
                    report += $"{Environment.NewLine}{Environment.NewLine}";
                }

                report += $"Element Key: {element.ElementKey} is not unique within the game files.";
            }
        }
    }
}