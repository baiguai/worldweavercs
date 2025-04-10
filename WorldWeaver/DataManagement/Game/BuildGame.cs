﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.Game
{
    public class BuildGame
    {

        public int depth = 0;
        public List<Classes.Element> elementsToInsert = new List<Classes.Element>();
        public int commitSize = 500;
        public bool inComments = false;

        public bool CreateDatabase(string game_key)
        {
            var success = false;
            var gameFile = $"{Environment.CurrentDirectory}/Games/{game_key.FileSafe()}.db";

            if (File.Exists(gameFile))
            {
                File.Delete(gameFile);
            }

            File.Create(gameFile);

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            Console.WriteLine("Creating the game database...");

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createDbQuery = $@"
--
-- File generated with SQLiteStudio v3.4.4 on Sun Feb 18 00:05:48 2024
--
-- Text encoding used: UTF-8
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: debuglog
DROP TABLE IF EXISTS debuglog;

CREATE TABLE IF NOT EXISTS debuglog (
    DebuglogId TEXT (100) PRIMARY KEY
                           UNIQUE
                           NOT NULL,
    Type        TEXT (100) NOT NULL,
    ParentKey  TEXT (200) NOT NULL,
    ParentType TEXT (100),
    CommandIn  TEXT (500),
    Output      BLOB,
    Logic       BLOB,
    Message     BLOB,
    CreateDate TEXT (10)  NOT NULL,
    UpdateDate TEXT (10)  NOT NULL
);

-- Table: element
DROP TABLE IF EXISTS element;

CREATE TABLE IF NOT EXISTS element (
    ElementType TEXT (200) NOT NULL,
    ElementKey  TEXT (300) PRIMARY KEY
                            UNIQUE
                            NOT NULL,
    Name         TEXT (300),
    ParentKey   TEXT (300) NOT NULL,
    Syntax       TEXT (300),
    Logic        BLOB,
    Output       BLOB,
    Tags         BLOB,
    Repeat       TEXT (200),
    RepeatIndex NUMERIC DEFAULT (-1),
    Template     TEXT (300),
    Active       TEXT (100) DEFAULT ('true'),
    Sort         NUMERIC    NOT NULL,
    CreateDate  TEXT (10)  NOT NULL,
    UpdateDate  TEXT (10)  NOT NULL
);

-- Table: gamestate
DROP TABLE IF EXISTS gamestate;

CREATE TABLE IF NOT EXISTS gamestate (
    GamestateId TEXT (200) PRIMARY KEY
                            UNIQUE
                            NOT NULL,
    GameStarted INTEGER    DEFAULT (0) 
                            NOT NULL,
    TimeHour    INTEGER    DEFAULT (12)
                            NOT NULL,
    TimeMinute  INTEGER    DEFAULT (0)
                            NOT NULL,
    TotalDays   INTEGER    DEFAULT (0)
                            NOT NULL,
    MissionDays INTEGER    DEFAULT (0)
                            NOT NULL,
    CreateDate  TEXT (10)  NOT NULL,
    UpdateDate  TEXT (10)  NOT NULL
);

-- Table: helpsys
DROP TABLE IF EXISTS helpsys;

CREATE TABLE IF NOT EXISTS helpsys (
    TopicId TEXT (200) PRIMARY KEY
                            UNIQUE
                            NOT NULL,
    Title       TEXT(400)  NOT NULL,
    Tags        BLOB       NOT NULL,
    Related     BLOB       NOT NULL,
    Article     BLOB       NOT NULL,
    CreateDate  TEXT (10)  NOT NULL,
    UpdateDate  TEXT (10)  NOT NULL
);

-- Table: note
DROP TABLE IF EXISTS note;

CREATE TABLE note (
    NoteKey  TEXT (255) PRIMARY KEY
                        UNIQUE
                        NOT NULL,
    NoteText BLOB       NOT NULL
);


INSERT INTO gamestate (
  GamestateId,
  GameStarted,
  TimeHour,
  TimeMinute,
  CreateDate,
  UpdateDate
)
VALUES (
  '{Guid.NewGuid().ToString()}',
  0,
  '{Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "init_hour"))}',
  '{Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "init_minute"))}',
  '{DateTime.Now.FormatDate()}',
  '{DateTime.Now.FormatDate()}'
);


-- Index: ix_debuglog_CommandIn
DROP INDEX IF EXISTS ix_debuglog_CommandIn;

CREATE INDEX IF NOT EXISTS ix_debuglog_CommandIn ON debuglog (
    CommandIn
);


-- Index: ix_debuglog_DebuglogId
DROP INDEX IF EXISTS ix_debuglog_DebuglogId;

CREATE UNIQUE INDEX IF NOT EXISTS ix_debuglog_DebuglogId ON debuglog (
    DebuglogId
);


-- Index: ix_debuglog_Output
DROP INDEX IF EXISTS ix_debuglog_Output;

CREATE INDEX IF NOT EXISTS ix_debuglog_Output ON debuglog (
    Output
);


-- Index: ix_debuglog_ParentKey
DROP INDEX IF EXISTS ix_debuglog_ParentKey;

CREATE INDEX IF NOT EXISTS ix_debuglog_ParentKey ON debuglog (
    ParentKey
);


-- Index: ix_debuglog_ParentType
DROP INDEX IF EXISTS ix_debuglog_ParentType;

CREATE INDEX IF NOT EXISTS ix_debuglog_ParentType ON debuglog (
    ParentType
);


-- Index: ix_debuglog_Type
DROP INDEX IF EXISTS ix_debuglog_Type;

CREATE INDEX IF NOT EXISTS ix_debuglog_Type ON debuglog (
    Type
);


-- Index: ix_element_ElementKey
DROP INDEX IF EXISTS ix_element_ElementKey;

CREATE UNIQUE INDEX IF NOT EXISTS ix_element_ElementKey ON element (
    ElementKey
);


-- Index: ix_element_ElementType
DROP INDEX IF EXISTS ix_element_ElementType;

CREATE INDEX IF NOT EXISTS ix_element_ElementType ON element (
    ElementType
);


-- Index: ix_ElementName
DROP INDEX IF EXISTS ix_ElementName;

CREATE INDEX IF NOT EXISTS ix_ElementName ON element (
    Name
);


-- Index: ix_element_ParentKey
DROP INDEX IF EXISTS ix_element_ParentKey;

CREATE INDEX IF NOT EXISTS ix_element_ParentKey ON element (
    ParentKey
);


-- Index: ix_element_Syntax
DROP INDEX IF EXISTS ix_element_Syntax;

CREATE INDEX IF NOT EXISTS ix_element_Syntax ON element (
    Syntax
);


-- Index: ix_element_Tags
DROP INDEX IF EXISTS ix_element_Tags;

CREATE INDEX IF NOT EXISTS ix_element_Tags ON element (
    Tags
);


DROP INDEX IF EXISTS ix_topicid;

CREATE INDEX IF NOT EXISTS ix_topicid ON helpsys (
    TopicId
);

DROP INDEX IF EXISTS ix_title;

CREATE INDEX IF NOT EXISTS ix_title ON helpsys (
    Title
);

DROP INDEX IF EXISTS ix_article;

CREATE INDEX IF NOT EXISTS ix_article ON helpsys (
    Article
);

DROP INDEX IF EXISTS ix_tags;

CREATE INDEX IF NOT EXISTS ix_tags ON helpsys (
    Tags
);

DROP INDEX IF EXISTS ix_related;

CREATE INDEX IF NOT EXISTS ix_related ON helpsys (
    Related
);


-- Index: ix_note_NoteKey
DROP INDEX IF EXISTS ix_note_NoteKey;

CREATE UNIQUE INDEX ix_note_NoteKey ON note (
    NoteKey
);

-- Index: ix_note_NoteText
DROP INDEX IF EXISTS ix_note_NoteText;

CREATE INDEX ix_note_NoteText ON note (
    NoteText
);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
                ";

                using (SqliteCommand command = new SqliteCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            success = true;

            return success;
        }

        internal bool LoadGame(string game_key)
        {
            var success = false;
            var gameFile = $"./Games/{game_key.FileSafe()}.db";
            var gameDirString = game_key.FileSafe();
            var gameDirectory = $"./GameSources/{gameDirString}/";

            if (File.Exists(gameFile))
            {
                File.Delete(gameFile);
            }

            CreateDatabase(game_key);

            Console.WriteLine("Loading the game files...");
            success = LoadGameFiles(gameFile, gameDirectory, gameDirectory);
            Console.WriteLine("Fixing the link elements...");
            success = FixLinkElements(gameFile);

            return success;
        }

        private bool LoadGameFiles(string gameFile, string rootDirectory, string curDirectory)
        {
            bool success = false;
            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            foreach (var dir in Directory.GetDirectories(curDirectory))
            {
                LoadGameFiles(gameFile, rootDirectory, dir);
            }

            foreach (string file in Directory.GetFiles(curDirectory))
            {
                if (Directory.Exists(file))
                {
                    success = LoadGameFiles(gameFile, rootDirectory, file);
                }
                else if (Path.GetExtension(file).Equals(".hlp"))
                {
                    LoadHelpFile(connectionString, file);
                }
                else if (Path.GetExtension(file).Equals(".nrmn"))
                {
                    List<string> lines = ProcessFile(rootDirectory, file, new HashSet<string>());

                    var allLines = "";

                    foreach (var curLine in lines)
                    {
                        if (!allLines.Equals(""))
                        {
                            allLines += Environment.NewLine;
                        }
                        allLines += curLine;
                    }

                    success = ParseElement(connectionString, lines, "root", 0) > -1;
                }
            }

            return success;
        }

        private void LoadHelpFile(string connectionString, string file)
        {
            var id = Guid.NewGuid().ToString();
            var title = "";
            var tags = "";
            var related = "";
            var article = "";
            foreach (var line in File.ReadLines(file))
            {
                if (line.Trim().Length > 7 && line.Trim().StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                {
                    title = line.Replace("title:", "", StringComparison.OrdinalIgnoreCase).Trim();
                }
                else if (line.Trim().Length > 6 && line.Trim().StartsWith("tags:", StringComparison.OrdinalIgnoreCase))
                {
                    tags = line.Replace("tags:", "", StringComparison.OrdinalIgnoreCase).Trim();
                }
                else if (line.Trim().Length > 9 && line.Trim().StartsWith("related:", StringComparison.OrdinalIgnoreCase))
                {
                    related = line.Replace("related:", "", StringComparison.OrdinalIgnoreCase).Trim();
                }
                else
                {
                    if (!article.Equals(""))
                    {
                        article+= System.Environment.NewLine;
                    }
                    article+= line;
                }
            }

            if (title.Equals("") || article.Equals(""))
            {
                Console.WriteLine($"The help file: {file} is malformed.");
                return;
            }

            SaveHelpTopic(connectionString, id, title, tags, related, article);
        }

        private List<string> ProcessFile(string rootDirectory, string filePath, HashSet<string> visitedFiles)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Warning: File not found - {filePath}");
                return new List<string> { $"// Missing file: {filePath}" };
            }

            // Prevent infinite loops from circular references
            if (visitedFiles.Contains(filePath))
            {
                Console.WriteLine($"Warning: Circular reference detected - {filePath}");
                return new List<string> { $"// Circular reference: {filePath}" };
            }

            visitedFiles.Add(filePath);

            string pattern = @"\{inject, logic=(?<logicPath>.+?)\s*\}";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var result = new List<string>();

            foreach (var line in File.ReadLines(filePath))
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    string logicPath = match.Groups["logicPath"].Value.Trim();
                    string fullPath = "";

                    if (logicPath.StartsWith("./"))
                    {
                        fullPath = Path.Combine(Directory.GetParent(filePath).ToString(), logicPath.Substring(2));
                    }
                    else
                    {
                        fullPath = Path.Combine(Path.GetFullPath(rootDirectory), logicPath);
                        // string dir = Path.GetDirectoryName(filePath) ?? "";
                        // fullPath = Path.Combine(dir, logicPath);
                    }
                    
                    // Recursively process the referenced file
                    var injectedContent = ProcessFile(rootDirectory, fullPath, visitedFiles);
                    result.AddRange(injectedContent);
                }
                else
                {
                    result.Add(line);
                }
            }

            visitedFiles.Remove(filePath);
            return result;
        }

        private int ParseElement(string connectionString, List<string> lines, string parentKey, int startLine) // @place
        {
            var success = false;
            var currentDepth = depth;
            Classes.Element element = null;
            var initRow = startLine;
            var currentRow = startLine;

            for (int ix = startLine; ix < lines.Count; ix++)
            {
                currentRow = ix;
                var line = lines[ix].Trim();

                if (!ParsableRow(line))
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

                            success = AddElement(connectionString, element);
                            MainClass.logger.WriteToLog($"Returning row: {currentRow}", Logger.LogTypes.BuildGame);
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
                        ix = ParseElement(connectionString, lines, element.ElementKey, ix);
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
                    case string s when line.ToLower().StartsWith("type=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.ElementType = line.Replace("type=", "", StringComparison.OrdinalIgnoreCase).ToLower().SqlSafe().FixElementType();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("key=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.ElementKey = line.Replace("key=", "", StringComparison.OrdinalIgnoreCase).Replace(' ', '_').SqlSafe();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("name=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.Name = line.Replace("name=", "", StringComparison.OrdinalIgnoreCase).SqlSafe();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("parent=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.ParentKey = line.Replace("parent=", "", StringComparison.OrdinalIgnoreCase).SqlSafe();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("syntax=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.Syntax = line.Replace("syntax=", "", StringComparison.OrdinalIgnoreCase).SqlSafe();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("logic=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            ix = GetFieldValue(element, lines, "logic", ix);
                            element.Logic = ParseMultilineField(element.Logic);
                        }
                        break;

                    case string s when line.ToLower().StartsWith("repeat=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            ix = GetFieldValue(element, lines, "repeat", ix);
                            element.Repeat = line.Replace("repeat=", "", StringComparison.OrdinalIgnoreCase).ToLower().SqlSafe();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("output=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            ix = GetFieldValue(element, lines, "output", ix);
                            element.Output = ParseMultilineField(element.Output);
                        }
                        break;

                    case string s when line.ToLower().StartsWith("tags=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            ix = GetFieldValue(element, lines, "tags", ix);
                        }
                        break;

                    case string s when line.ToLower().StartsWith("template=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            ix = GetFieldValue(element, lines, "template", ix);
                        }
                        break;

                    case string s when line.ToLower().StartsWith("active=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.Active = line.Replace("active=", "", StringComparison.OrdinalIgnoreCase).ToLower();
                        }
                        break;

                    case string s when line.ToLower().StartsWith("sort=", StringComparison.OrdinalIgnoreCase):
                        if (element != null)
                        {
                            element.Sort = Convert.ToInt32(line.Replace("sort=", "", StringComparison.OrdinalIgnoreCase));
                        }
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

                            success = AddElement(connectionString, element);
                            element = null;
                        }
                        break;
                }
            }

            if (element != null) // @place
            {
                success = AddElement(connectionString, element);
            }

            SaveElements(connectionString);

            return currentRow;
        }

        private int GetFieldValue(Classes.Element element, List<string> lines, string fieldName, int startRow)
        {
            var currentRow = startRow;
            var autoBreak = false;
            var propertyValue = "";

            for (var ix = startRow; ix < lines.Count; ix++)
            {
                var line = lines[ix];

                if (ix == startRow)
                {
                    line = line.Trim();
                    line = line.Replace($"{fieldName}=", "", StringComparison.OrdinalIgnoreCase);
                    if (!line.StartsWith("{"))
                    {
                        propertyValue = line;
                        break;
                    }
                    else
                    {
                        if (line.Replace("{", "").Equals("@"))
                        {
                            autoBreak = true;
                        }
                        continue;
                    }
                }

                if (line.Trim().Equals("}"))
                {
                    currentRow = ix++;
                    break;
                }
                else
                {
                    if (!propertyValue.Equals("") && autoBreak)
                    {
                        propertyValue += Environment.NewLine;
                    }

                    propertyValue += line;
                }
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            fieldName = textInfo.ToTitleCase(fieldName);

            PropertyInfo propertyInfo = element.GetType().GetProperty(fieldName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(element, Convert.ChangeType(propertyValue.SqlSafe(), propertyInfo.PropertyType), null);
            }

            return currentRow;
        }

        private Classes.Element ParseInlineProperties(Classes.Element element, string dataRow)
        {
            dataRow = dataRow.Replace("{", "");
            var arr = dataRow.Split(',');

            // The first field can be the 'type' shorthand
            // {action, logic="...", tags="..." }
            if (!arr[0].Contains("="))
            {
                element.ElementType = arr[0].Trim().FixElementType();
            }

            foreach (var pair in arr)
            {
                var pairArr = pair.Trim().Split('=', 2);

                if (pairArr.Length == 2)
                {
                    switch (pairArr[0].Trim().ToLower())
                    {
                        case "key":
                            element.ElementKey = pairArr[1].Trim().Replace(' ', '_');
                            break;

                        case "type":
                            element.ElementType = pairArr[1].ToLower().Trim().FixElementType();
                            break;

                        case "name":
                            element.Name = pairArr[1].Trim();
                            break;

                        case "parent":
                            element.ParentKey = pairArr[1].Trim();
                            break;

                        case "syntax":
                            element.Syntax = pairArr[1].Trim();
                            break;

                        case "repeat":
                            element.Repeat = pairArr[1].ToLower().Trim();
                            break;

                        case "logic":
                            element.Logic = ParseMultilineField(pairArr[1].Trim());
                            break;

                        case "output":
                            element.Output = ParseMultilineField(pairArr[1].Trim());
                            break;

                        case "tags":
                            element.Tags = pairArr[1].Trim();
                            break;

                        case "template":
                            element.Template = pairArr[1].Trim();
                            break;

                        case "active":
                            element.Active = pairArr[1].ToLower().Trim();
                            break;

                        case "sort":
                            if (element.ElementType.Equals("devnote"))
                            {
                                element.Sort = 9999;
                            }
                            else
                            {
                                element.Sort = Convert.ToInt32(pairArr[1].Trim());
                            }
                            break;
                    }
                }
            }

            return element;
        }

        private string ParseMultilineField(string output)
        {
            var retVal = output;

            return retVal.SqlSafe();
        }

        private string ParseLogicField(string logic)
        {
            var output = logic;

            return output.SqlSafe();
        }

        public bool ParsableRow(string dataRow)
        {
            if (dataRow.Equals(""))
            {
                return false;
            }

            if (dataRow.Trim().Equals("*/"))
            {
                inComments = false;
                return false;
            }

            if (inComments)
            {
                return false;
            }

            if (dataRow.Trim().Equals("/*"))
            {
                inComments = true;
                return false;
            }

            if (dataRow.Substring(0, 1).Equals("!"))
            {
                return false;
            }

            if (dataRow.Trim().Length > 1 && dataRow.Trim().Substring(0, 2).Equals("//"))
            {
                return false;
            }

            return true;
        }

        public bool AddElement(string connectionString, Classes.Element element)
        {
            var success = false;

            if (elementsToInsert.Count() > commitSize)
            {
                success = SaveElements(connectionString);
            }
            else
            {
                elementsToInsert.Add(element);
            }

            return success;
        }

        public bool SaveElement(Classes.Element element)
        {
            var gameFile = $"Games/{MainClass.gameDb}.db";
            var connectionString = $"Data Source={gameFile};Cache=Shared;";
            return SaveElement(connectionString, element);
        }
        public bool SaveElement(string connectionString, Classes.Element element)
        {
            elementsToInsert.Clear();
            elementsToInsert.Add(element);
            var success = SaveElements(connectionString);

            return success;
        }

        public bool SaveElements(string connectionString)
        {
            var success = false;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createDbQuery = $@"
INSERT INTO element (
    ElementType,
    ElementKey,
    Name,
    ParentKey,
    Syntax,
    Logic,
    Output,
    Tags,
    Template,
    Repeat,
    Active,
    Sort,
    CreateDate,
    UpdateDate
)
VALUES";

                if (elementsToInsert == null || elementsToInsert.Count < 1)
                {
                    return true;
                }

                var lastElem = elementsToInsert.Last();
                foreach (var e in elementsToInsert)
                {
                    createDbQuery += $@"
( '{e.ElementType}','{e.ElementKey}','{e.Name}','{e.ParentKey}','{e.Syntax}','{e.Logic}','{e.Output}','{e.Tags}','{e.Template}','{e.Repeat}','{e.Active}','{e.Sort}','{e.CreateDate}','{e.UpdateDate}' )";

                    if (e.Equals(lastElem))
                    {
                        createDbQuery += ";";

                        if (!createDbQuery.Equals(""))
                        {
                            try
                            {
                                using (SqliteCommand command = new SqliteCommand(createDbQuery, connection))
                                {
                                    command.ExecuteNonQuery();
                                    command.Dispose();
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(createDbQuery);
                                throw;
                            }

                            elementsToInsert.Clear();
                            connection.Close();
                            connection.Dispose();
                            return true;
                        }
                    }
                    else
                    {
                        createDbQuery += ",";
                    }

                    MainClass.logger.WriteToLog($"Element Type: {e.ElementType}, Key: {e.ElementKey}, Parent: {e.ParentKey}", Logger.LogTypes.BuildGame);
                }
            }

            success = true;
            return success;
        }

        private bool FixLinkElements(string gameFile)
        {
            var connectionString = $"Data Source={gameFile};Cache=Shared;";
            var success = false;

            success = UpdateLinkChildren(connectionString);
            if (!success)
            {
                return false;
            }
            success = UpdateLinks(connectionString);

            return success;
        }

        private bool UpdateLinks(string connectionString)
        {
            var updateQuery = $@"
DELETE FROM
    element
WHERE 1=1
    AND ElementType = 'link'
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return true;
        }

        private bool UpdateLinkChildren(string connectionString)
        {
            var updateQuery = $@"
UPDATE
    element
SET
    ParentKey = 
    (
        SELECT
            p.ParentKey
        FROM
            element p
        WHERE 1=1
            AND p.ElementType = 'link'
            AND p.ElementKey = element.ParentKey
    )
WHERE
    EXISTS
    (
        SELECT
            1
        FROM
            element p
        WHERE 1=1
            AND p.ElementType = 'link'
            AND p.ElementKey = element.ParentKey
    )
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return true;
        }

        private bool SaveHelpTopic(string connectionString, string id, string title, string tags, string related, string article)
        {
            id = id.SqlSafe();
            title = title.SqlSafe();
            tags = tags.SqlSafe();
            related = related.SqlSafe();
            article = article.SqlSafe();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createDbQuery = $@"
INSERT INTO helpsys (
    TopicId,
    Title,
    Tags,
    Related,
    Article,
    CreateDate,
    UpdateDate
)
VALUES";

                createDbQuery += $@"
( '{id}','{title}','{tags}','{related}','{article}','{DateTime.Now.FormatDate()}','{DateTime.Now.FormatDate()}' )";

                try
                {
                    using (SqliteCommand command = new SqliteCommand(createDbQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine(createDbQuery);
                    throw;
                }

                elementsToInsert.Clear();
                connection.Close();
                connection.Dispose();
                return true;
            }
        }
    }
}
