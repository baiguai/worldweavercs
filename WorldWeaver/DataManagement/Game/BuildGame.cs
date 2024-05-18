using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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


INSERT INTO gamestate (
  GamestateId,
  GameStarted,
  TimeHour,
  TimeMinute,
  CreateDate,
  UpdateDate
)
VALUES (
  '{Guid.NewGuid()}',
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

            success = LoadGameFiles(gameFile, gameDirectory);
            success = FixLinkElements(gameFile);

            return success;
        }

        private bool LoadGameFiles(string gameFile, string gameDirectory)
        {
            var success = false;
            var connectionString = $"Data Source={gameFile};Cache=Shared;";

            foreach (var dir in Directory.GetDirectories(gameDirectory))
            {
                LoadGameFiles(gameFile, dir);
            }

            foreach (string file in Directory.GetFiles(gameDirectory))
            {
                if (Directory.Exists(file))
                {
                    success = LoadGameFiles(gameFile, file);
                }
                else if (Path.GetExtension(file).Equals(".nrmn"))
                {
                    var lines = File.ReadAllLines(file).ToList();
                    success = (ParseElement(connectionString, lines, "root", 0) > -1);
                }
            }

            return success;
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

                            success = LoadElement(connectionString, element);
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
                        element.ElementType = line.Replace("type=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("key=", StringComparison.OrdinalIgnoreCase):
                        element.ElementKey = line.Replace("key=", "").Replace(' ', '_').SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("name=", StringComparison.OrdinalIgnoreCase):
                        element.Name = line.Replace("name=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("parent=", StringComparison.OrdinalIgnoreCase):
                        element.ParentKey = line.Replace("parent=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("syntax=", StringComparison.OrdinalIgnoreCase):
                        element.Syntax = line.Replace("syntax=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("logic=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "logic", ix);
                        element.Logic = ParseMultilineField(element.Logic);
                        break;

                    case string s when line.ToLower().StartsWith("repeat=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "repeat", ix);
                        element.Repeat = line.Replace("repeat=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("output=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "output", ix);
                        element.Output = ParseMultilineField(element.Output);
                        break;

                    case string s when line.ToLower().StartsWith("tags=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "tags", ix);
                        break;

                    case string s when line.ToLower().StartsWith("active=", StringComparison.OrdinalIgnoreCase):
                        element.Active = line.Replace("active=", "");
                        break;

                    case string s when line.ToLower().StartsWith("sort=", StringComparison.OrdinalIgnoreCase):
                        element.Sort = Convert.ToInt32(line.Replace("sort=", ""));
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

                            success = LoadElement(connectionString, element);
                            element = null;
                        }
                        break;
                }
            }

            if (element != null) // @place
            {
                success = LoadElement(connectionString, element);
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
                var line = lines[ix].Trim();

                if (ix == startRow)
                {
                    line = line.Replace($"{fieldName}=", "");
                    if (!line.StartsWith("{", StringComparison.OrdinalIgnoreCase))
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

                if (line.Equals("}"))
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
            propertyInfo.SetValue(element, Convert.ChangeType(propertyValue.SqlSafe(), propertyInfo.PropertyType), null);

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

                        case "type":
                            element.ElementType = pairArr[1].Trim();
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
                            element.Repeat = pairArr[1].Trim();
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

                        case "active":
                            element.Active = pairArr[1].Trim();
                            break;

                        case "sort":
                            element.Sort = Convert.ToInt32(pairArr[1].Trim());
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

            if (dataRow.Substring(0, 1).Equals("!"))
            {
                return false;
            }

            if (dataRow.Length > 1 && dataRow.Substring(0, 2).Equals("//"))
            {
                return false;
            }

            return true;
        }

        private string PerformLogic(string logic)
        {
            var output = logic;

            return output;
        }

        private bool LoadElement(string connectionString, Classes.Element element)
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

        private bool SaveElements(string connectionString)
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
    Repeat,
    Active,
    Sort,
    CreateDate,
    UpdateDate
)
VALUES";

                if (elementsToInsert.Count < 1)
                {
                    return true;
                }

                var lastElem = elementsToInsert.Last();
                foreach (var e in elementsToInsert)
                {
                    createDbQuery += $@"
('{e.ElementType}',
'{e.ElementKey}',
'{e.Name}',
'{e.ParentKey}',
'{e.Syntax}',
'{e.Logic}',
'{e.Output}',
'{e.Tags}',
'{e.Repeat}',
'{e.Active}',
'{e.Sort}',
'{e.CreateDate}',
'{e.UpdateDate}')";

                    if (e.Equals(lastElem))
                    {
                        createDbQuery += ";";
                    }
                    else
                    {
                        createDbQuery += ",";
                    }

                    MainClass.logger.WriteToLog($"Element Type: {e.ElementType}, Key: {e.ElementKey}, Parent: {e.ParentKey}", Logger.LogTypes.BuildGame);
                }

                using (SqliteCommand command = new SqliteCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            success = true;
            elementsToInsert.Clear();

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
    }
}