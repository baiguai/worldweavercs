using System;
using System.Collections.Generic;
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
            var gameFile = $"Games/{game_key.FileSafe()}.db";

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
    debuglog_id TEXT (100) PRIMARY KEY
                           UNIQUE
                           NOT NULL,
    type        TEXT (100) NOT NULL,
    ParentKey  TEXT (200) NOT NULL,
    parent_type TEXT (100),
    command_in  TEXT (500),
    output      BLOB,
    logic       BLOB,
    message     BLOB,
    create_date TEXT (10)  NOT NULL,
    update_date TEXT (10)  NOT NULL
);


-- Table: element
DROP TABLE IF EXISTS element;

CREATE TABLE IF NOT EXISTS element (
    ElementType TEXT (200) NOT NULL,
    ElementKey  TEXT (300) PRIMARY KEY
                            UNIQUE
                            NOT NULL,
    name         TEXT (300),
    ParentKey   TEXT (300) NOT NULL,
    location     TEXT (300),
    syntax       TEXT (300),
    logic        BLOB,
    output       BLOB,
    tags         BLOB,
    repeat_type  TEXT (200),
    repeat_index NUMERIC DEFAULT (-1),
    active       TEXT (100) DEFAULT ('1'),
    sort         NUMERIC    NOT NULL,
    create_date  TEXT (10)  NOT NULL,
    update_date  TEXT (10)  NOT NULL
);

-- Table: gamestate
DROP TABLE IF EXISTS gamestate;

CREATE TABLE IF NOT EXISTS gamestate (
    gamestate_id TEXT (200) PRIMARY KEY
                            UNIQUE
                            NOT NULL,
    game_started INTEGER    DEFAULT (0) 
                            NOT NULL,
    create_date  TEXT (10)  NOT NULL,
    update_date  TEXT (10)  NOT NULL
);


INSERT INTO gamestate (
  gamestate_id,
  game_started,
  create_date,
  update_date
)
VALUES (
  '{Guid.NewGuid()}',
  0,
  '{DateTime.Now.FormatDate()}',
  '{DateTime.Now.FormatDate()}'
);


-- Index: ix_debuglog_command_in
DROP INDEX IF EXISTS ix_debuglog_command_in;

CREATE INDEX IF NOT EXISTS ix_debuglog_command_in ON debuglog (
    command_in
);


-- Index: ix_debuglog_debuglog_id
DROP INDEX IF EXISTS ix_debuglog_debuglog_id;

CREATE UNIQUE INDEX IF NOT EXISTS ix_debuglog_debuglog_id ON debuglog (
    debuglog_id
);


-- Index: ix_debuglog_output
DROP INDEX IF EXISTS ix_debuglog_output;

CREATE INDEX IF NOT EXISTS ix_debuglog_output ON debuglog (
    output
);


-- Index: ix_debuglog_ParentKey
DROP INDEX IF EXISTS ix_debuglog_ParentKey;

CREATE INDEX IF NOT EXISTS ix_debuglog_ParentKey ON debuglog (
    ParentKey
);


-- Index: ix_debuglog_parent_type
DROP INDEX IF EXISTS ix_debuglog_parent_type;

CREATE INDEX IF NOT EXISTS ix_debuglog_parent_type ON debuglog (
    parent_type
);


-- Index: ix_debuglog_type
DROP INDEX IF EXISTS ix_debuglog_type;

CREATE INDEX IF NOT EXISTS ix_debuglog_type ON debuglog (
    type
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


-- Index: ix_element_name
DROP INDEX IF EXISTS ix_element_name;

CREATE INDEX IF NOT EXISTS ix_element_name ON element (
    name
);


-- Index: ix_element_ParentKey
DROP INDEX IF EXISTS ix_element_ParentKey;

CREATE INDEX IF NOT EXISTS ix_element_ParentKey ON element (
    ParentKey
);

-- Index: ix_element_ParentKey
DROP INDEX IF EXISTS ix_element_location;

CREATE INDEX IF NOT EXISTS ix_element_location ON element (
    location
);

-- Index: ix_element_syntax
DROP INDEX IF EXISTS ix_element_syntax;

CREATE INDEX IF NOT EXISTS ix_element_syntax ON element (
    syntax
);


-- Index: ix_element_tags
DROP INDEX IF EXISTS ix_element_tags;

CREATE INDEX IF NOT EXISTS ix_element_tags ON element (
    tags
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
            var gameFile = $"Games/{game_key.FileSafe()}.db";
            var gameDirString = game_key.FileSafe();
            var gameDirectory = $"GameSources/{gameDirString}/";

            if (File.Exists(gameFile))
            {
                File.Delete(gameFile);
            }

            CreateDatabase(game_key);

            success = LoadGameFiles(gameFile, gameDirectory);

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

        private int ParseElement(string connectionString, List<string> lines, string parentKey, int startLine)
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
                        line = line.Substring(0, line.Length - 1);
                    }

                    if (element == null)
                    {
                        element = new Classes.Element
                        {
                            ParentKey = parentKey,
                            Sort = ix
                        };

                        if (line.Length > 1)
                        {
                            element = ParseInlineProperties(element, line);
                            if (element.ParentKey == null)
                            {
                                element.ParentKey = parentKey;
                            }
                        }

                        if (singleLine)
                        {
                            if (element.ElementKey == null)
                            {
                                element.ElementKey = Guid.NewGuid().ToString();
                            }

                            success = LoadElement(connectionString, element);
                            element = null;
                        }

                        continue;
                    }
                    else
                    {
                        depth++;
                        if (element.ElementKey == null)
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
                        element.ElementKey = line.Replace("key=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("name=", StringComparison.OrdinalIgnoreCase):
                        element.Name = line.Replace("name=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("parent=", StringComparison.OrdinalIgnoreCase):
                        element.ParentKey = line.Replace("parent=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("location=", StringComparison.OrdinalIgnoreCase):
                        element.Location = line.Replace("location=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("syntax=", StringComparison.OrdinalIgnoreCase):
                        element.Syntax = line.Replace("syntax=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("logic=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "logic", ix);
                        element.Logic = ParseLogicField(element.Logic);
                        break;

                    case string s when line.ToLower().StartsWith("repeat=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "repeat_type", ix);
                        element.Repeat = line.Replace("repeat=", "").SqlSafe();
                        break;

                    case string s when line.ToLower().StartsWith("output=", StringComparison.OrdinalIgnoreCase):
                        ix = GetFieldValue(element, lines, "output", ix);
                        element.Output = ParseOutputField(element.Output);
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
                            return currentRow-1;
                        }

                        if (depth == currentDepth)
                        {
                            if (element.ElementKey == null)
                            {
                                element.ElementKey = Guid.NewGuid().ToString();
                            }

                            success = LoadElement(connectionString, element);
                            element = null;
                        }
                        break;
                }
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
                        propertyValue = line.OutputFormat();
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

                    propertyValue += line.OutputFormat();
                }
            }

            PropertyInfo propertyInfo = element.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(element, Convert.ChangeType(propertyValue.SqlSafe(), propertyInfo.PropertyType), null);

            return currentRow;
        }

        private Classes.Element ParseInlineProperties(Classes.Element element, string dataRow)
        {
            dataRow = dataRow.Replace("{", "");
            var arr = dataRow.Split(',');

            foreach(var pair in arr)
            {
                var pairArr = pair.Trim().Split('=');
                if (pairArr.Length == 2)
                { 
                    switch (pairArr[0].Trim().ToLower())
                    {
                        case "key":
                            element.ElementKey = pairArr[1].Trim();
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

                        case "location":
                            element.Location = pairArr[1].Trim();
                            break;

                        case "syntax":
                            element.Syntax = pairArr[1].Trim();
                            break;

                        case "repeat":
                            element.Repeat = pairArr[1].Trim();
                            break;

                        case "logic":
                            element.Logic = ParseLogicField(pairArr[1].Trim());
                            break;

                        case "output":
                            element.Output = pairArr[1].Trim();
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

        private string ParseOutputField(string output)
        {
            var retVal = output;

            if (retVal.Contains("[rand:"))
            {
                var tmp = retVal.Replace("[rand:", "").Replace("]", "");
                var range = tmp.Split('|');
                if (range.Length == 2)
                {
                    Random rnd = new Random((int)DateTime.Now.Ticks);
                    retVal = rnd.Next(Convert.ToInt32(range[0]), Convert.ToInt32(range[1])).ToString();
                }
            }

            return retVal.SqlSafe();
        }

        private string ParseLogicField(string logic)
        {
            var output = logic;

            return output.SqlSafe();
        }

        private bool ParsableRow(string dataRow)
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
    element_type,
    element_key,
    name,
    parent_key,
    location,
    syntax,
    logic,
    output,
    tags,
    active,
    sort,
    create_date,
    update_date
)
VALUES";

                var lastElem = elementsToInsert.Last();
                foreach (var e in elementsToInsert)
                {
                    createDbQuery += $@"
('{e.ElementType}',
'{e.ElementKey}',
'{e.Name}',
'{e.ParentKey}',
'{e.Location}',
'{e.Syntax}',
'{e.Logic}',
'{e.Output}',
'{e.Tags}',
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
                }

                using (SqliteCommand command = new SqliteCommand(createDbQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            success = true;
            elementsToInsert.Clear();

            return success;
        }
    }
}