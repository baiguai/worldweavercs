using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class ValidationParser
    {
        public List<string> ElementKeys = new List<string>();

        public void ProcessGameValidation(string game_key)
        {
            var validRpt = "";
            var gameFile = $"./Games/{game_key.FileSafe()}.db";
            var gameDirString = game_key.FileSafe();
            var gameDirectory = $"./GameSources/{gameDirString}/";

            validRpt += $"{ValidateGameFiles(gameDirectory, validRpt)}";
            if (validRpt.Equals(""))
            {
                validRpt = "No issues found.";
            }

            MainClass.output.OutputText = validRpt;
            MainClass.output.MatchMade = true;
        }

        private string ValidateGameFiles(string gameDirectory, string validRpt)
        {
            foreach (var dir in Directory.GetDirectories(gameDirectory))
            {
                validRpt += ValidateGameFiles(dir, validRpt);
            }

            foreach (string file in Directory.GetFiles(gameDirectory))
            {
                if (Path.GetExtension(file).Equals(".nrmn"))
                {
                    var lines = File.ReadAllLines(file).ToList();

                    validRpt = DoDuplicateKeyCheck(lines, validRpt);
                }
            }

            return validRpt;
        }

        private string DoDuplicateKeyCheck(List<string> lines, string validRpt)
        {
            foreach (var line in lines)
            {
                string key = ProcessElementKey(line);

                if (!key.Equals(""))
                {
                    if (ElementKeys.Contains(key))
                    {
                        validRpt += $"Duplicate key found: {key}{Environment.NewLine}";
                    }
                    else
                    {
                        ElementKeys.Add(key);
                    }
                }
            }

            return validRpt;
        }

        private string ProcessElementKey(string line)
        {
            var key = "";

            key = GetInlineKey(line);

            if (key.Equals(""))
            {
                key = GetSingleLineKey(line);
            }

            return key;
        }

        private string GetInlineKey(string line)
        {
            var key = "";

            var arr = line.Split("key=");
            if (arr.Length == 2)
            {
                var arr2 = arr[1].Trim().Split(',');
                key = arr2[0].Trim();
            }

            return key;
        }

        private string GetSingleLineKey(string line)
        {
            var key = "";

            if (line.StartsWith("key="))
            {
                key = line.Replace("key=", "").Trim();
            }

            return key;
        }
    }
}