using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Element
    {
        public List<Classes.Element> GetElementsByType(string gameDb, string type)
        {
            var cachedElems = Tools.CacheManager.GetCachedElementByType(type);
            if (cachedElems.Count > 0)
            {
                if (cachedElems.Count == 1 && cachedElems[0] != null)
                {
                    return cachedElems;
                }
            }

            var selectQuery = $@"
SELECT
    ElementType,
    ElementKey,
    Name,
    ParentKey,
    Syntax,
    Logic,
    Output,
    Tags,
    Repeat,
    RepeatIndex,
    Active,
    Sort
FROM
    element
WHERE 1=1
    AND ElementType = @type
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@type",
                ParamValue = type
            });

            var output = GetElements(selectQuery, gameDb, parms);
            return output;
        }

        public List<Classes.Element> GetRandOutputElements(string gameDb)
        {
            var selectQuery = $@"
SELECT
    ElementType,
    ElementKey,
    Name,
    ParentKey,
    Syntax,
    Logic,
    Output,
    Tags,
    Repeat,
    RepeatIndex,
    Active,
    Sort
FROM
    element
WHERE 1=1
    AND Output LIKE '[rand:%'
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var output = GetElements(selectQuery, gameDb);
            return output;
        }

        internal bool SetElementParentKey(string gameDb, string key, string parentKey)
        {
            var output = false;

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            var updateQuery = $@"
UPDATE
    element
SET
    ParentKey = @newParentKey
WHERE 1=1
    AND ElementKey = @elementkey
    AND Active = 'true'
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@elementkey", key);
                    command.Parameters.AddWithValue("@newParentKey", parentKey);

                    command.ExecuteNonQuery();
                }

                connection.Close();
                output = true;
            }

            CacheManager.RefreshCache(gameDb);

            return output;
        }

        public Classes.Element GetElementByKey(string gameDb, string element_key)
        {
            var cachedElem = CacheManager.GetCachedElement(element_key);
            if (cachedElem != null)
            {
                return cachedElem;
            }

            var selectQuery = $@"
SELECT
    ElementType,
    ElementKey,
    Name,
    ParentKey,
    Syntax,
    Logic,
    Output,
    Tags,
    Repeat,
    RepeatIndex,
    Active,
    Sort
FROM
    element
WHERE 1=1
    AND ElementKey = @elementkey
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@elementkey",
                ParamValue = element_key
            });


            var output = GetElement(selectQuery, gameDb, parms);

            return output;
        }

        public List<Classes.Element> GetElementChildren(string gameDb, string parent_key)
        {
            var cachedElem = Tools.CacheManager.GetCachedElement(parent_key);
            if (cachedElem != null)
            {
                return cachedElem.Children;
            }

            var selectQuery = $@"
SELECT
    ElementType,
    ElementKey,
    Name,
    ParentKey,
    Syntax,
    Logic,
    Output,
    Tags,
    Repeat,
    RepeatIndex,
    Active,
    Sort
FROM
    element
WHERE 1=1
    AND ParentKey = @parentkey
    AND Active = 'true'
ORDER BY
    Sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@parentkey",
                ParamValue = parent_key
            });


            var output = GetElements(selectQuery, gameDb, parms);

            return output;
        }

        public string GetElementField(string gameDb, string element_key, string element_field)
        {
            var output = "";
            var selectQuery = $@"
SELECT
    {element_field} as Field
FROM
    element
WHERE 1=1
    AND ElementKey = @elementkey
    AND Active = 'true'
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@elementkey",
                ParamValue = element_key
            });

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return "";
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.ParamName, parm.ParamValue);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            output = reader.GetString(reader.GetOrdinal("Field"));
                            break;
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }


        public Classes.Element GetElement(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = new Classes.Element();

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.ParamName, parm.ParamValue);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var e = new Classes.Element();

                            e.ElementType = reader.GetString(reader.GetOrdinal("ElementType"));
                            e.ElementKey = reader.GetString(reader.GetOrdinal("ElementKey"));
                            if (reader["Name"] != DBNull.Value) { e.Name = reader.GetString(reader.GetOrdinal("Name")); }
                            e.ParentKey = reader.GetString(reader.GetOrdinal("ParentKey"));
                            if (reader["Syntax"] != DBNull.Value) { e.Syntax = reader.GetString(reader.GetOrdinal("Syntax")); }
                            if (reader["Logic"] != DBNull.Value) { e.Logic = reader.GetString(reader.GetOrdinal("Logic")); }
                            if (reader["Output"] != DBNull.Value) { e.Output = reader.GetString(reader.GetOrdinal("Output")); }
                            if (reader["Tags"] != DBNull.Value) { e.Tags = reader.GetString(reader.GetOrdinal("Tags")); }
                            if (reader["Repeat"] != DBNull.Value) { e.Repeat = reader.GetString(reader.GetOrdinal("Repeat")); }
                            e.RepeatIndex = reader.GetInt32(reader.GetOrdinal("RepeatIndex"));
                            e.Active = reader.GetString(reader.GetOrdinal("Active"));
                            e.Sort = reader.GetInt32(reader.GetOrdinal("Sort"));
                            e.Children = GetElementChildren(gameDb, reader.GetString(reader.GetOrdinal("ElementKey")));

                            output = e;
                            break;
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public List<Classes.Element> GetElements(string selectQuery, string gameDb)
        {
            return GetElements(selectQuery, gameDb, new List<DbParameter>());
        }
        public List<Classes.Element> GetElements(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = new List<Classes.Element>();

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.ParamName, parm.ParamValue);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var e = new Classes.Element();

                            e.ElementType = reader.GetString(reader.GetOrdinal("ElementType"));
                            e.ElementKey = reader.GetString(reader.GetOrdinal("ElementKey"));
                            if (reader["Name"] != DBNull.Value) { e.Name = reader.GetString(reader.GetOrdinal("Name")); }
                            e.ParentKey = reader.GetString(reader.GetOrdinal("ParentKey"));
                            if (reader["Syntax"] != DBNull.Value) { e.Syntax = reader.GetString(reader.GetOrdinal("Syntax")); }
                            if (reader["Logic"] != DBNull.Value) { e.Logic = reader.GetString(reader.GetOrdinal("Logic")); }
                            if (reader["Output"] != DBNull.Value) { e.Output = reader.GetString(reader.GetOrdinal("Output")); }
                            if (reader["Tags"] != DBNull.Value) { e.Tags = reader.GetString(reader.GetOrdinal("Tags")); }
                            if (reader["Repeat"] != DBNull.Value) { e.Repeat = reader.GetString(reader.GetOrdinal("Repeat")); }
                            e.RepeatIndex = reader.GetInt32(reader.GetOrdinal("RepeatIndex"));
                            e.Active = reader.GetString(reader.GetOrdinal("Active"));
                            e.Sort = reader.GetInt32(reader.GetOrdinal("Sort"));
                            e.Children = GetElementChildren(gameDb, reader.GetString(reader.GetOrdinal("ElementKey")));

                            output.Add(e);
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public List<Classes.SearchElement> GetSearchElements(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = new List<Classes.SearchElement>();

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.ParamName, parm.ParamValue);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var e = new Classes.SearchElement();

                            e.ElementKey = reader.GetString(reader.GetOrdinal("ElementKey"));
                            e.Name = reader.GetString(reader.GetOrdinal("Name"));
                            if (reader["Syntax"] != DBNull.Value) { e.Syntax = reader.GetString(reader.GetOrdinal("Syntax")); }
                            e.Active = reader.GetBoolean(reader.GetOrdinal("Active"));

                            output.Add(e);
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public bool SetElementField(string gameDb, string element_key, string field, string new_value, bool refreshCache = true)
        {
            var output = false;

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            var updateQuery = $@"
UPDATE
    element
SET
    {field} = @newvalue
WHERE 1=1
    AND ElementKey = @elementkey
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@elementkey", element_key);
                    command.Parameters.AddWithValue("@newvalue", new_value);

                    command.ExecuteNonQuery();
                }

                connection.Close();
                output = true;
            }

            if (refreshCache)
            {
                CacheManager.RefreshCache(gameDb);
            }

            return output;
        }

        public int GetRepeatIndex(string gameDb, string element_key)
        {
            var selectQuery = $@"
SELECT
    RepeatIndex
FROM
    element
WHERE 1=1
    AND ElementKey = @elementkey
    AND Active = 'true'
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@elementkey",
                ParamValue = element_key
            });


            var output = GetInt(selectQuery, gameDb, parms);

            return output;
        }

        private int GetInt(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = 0;

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.ParamName, parm.ParamValue);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            output = reader.GetInt32(0);
                            break;
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        internal string ElementLookup(string gameDb, string idValue)
        {
            var elem = GetElementByKey(gameDb, idValue);
            if (!elem.ElementKey.Equals(""))
            {
                return elem.ElementKey;
            }

            var elems = GetElementKeysBySyntax(gameDb, idValue);
            if (elems.Count == 1)
            {
                return elems.First();
            }

            return "";
        }

        internal List<string> GetElementKeysBySyntax(string gameDb, string idValue)
        {
            return GetElementKeysBySyntax(gameDb, idValue, false);
        }
        internal List<string> GetElementKeysBySyntax(string gameDb, string idValue, bool activeOnly)
        {
            var output = new List<string>();
            var allElems = GetElemsForSyntaxSearch(gameDb);

            foreach (var elem in allElems)
            {
                Regex rgx = new Regex(idValue, RegexOptions.IgnoreCase);

                if (rgx.IsMatch(elem.Syntax))
                {
                    if (!activeOnly || elem.Active)
                    {
                        output.Add(elem.ElementKey);
                    }
                }
            }
            if (output.Count == 0)
            {
                foreach (var elem in allElems)
                {
                    if (elem.Name.Contains(idValue))
                    {
                        if (!activeOnly || elem.Active)
                        {
                            output.Add(elem.ElementKey);
                        }
                    }
                }
            }

            return output;
        }

        private List<SearchElement> GetElemsForSyntaxSearch(string gameDb)
        {
            var selectQuery = $@"
SELECT
    ElementKey,
    Name,
    Syntax,
    Active
FROM
    element
WHERE 1=1
    AND Syntax IS NOT NULL
    AND Active = 'true'
;
            ";

            var parms = new List<DbParameter>();

            var output = GetSearchElements(selectQuery, gameDb, parms);

            return output;
        }

        internal void SetRandOutputElements(string gameDb)
        {
            var elems = GetRandOutputElements(gameDb);
            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                foreach (var elem in elems)
                {
                    if (elem.Output.Contains("[rand:"))
                    {
                        var tmp = elem.Output.Replace("[rand:", "").Replace("]", "");
                        var range = tmp.Split('|');
                        if (range.Length == 2)
                        {
                            Random rnd = new Random((int)DateTime.Now.Ticks);
                            elem.Output = rnd.Next(Convert.ToInt32(range[0]), Convert.ToInt32(range[1])).ToString();
                        }
                    }

                    var updateQuery = $@"
UPDATE
    element
SET
    Output = '{elem.Output}'
WHERE 1=1
    AND ElementKey = '{elem.ElementKey}'
    AND Active = 'true'
;
                    ";

                    using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
    }
}
