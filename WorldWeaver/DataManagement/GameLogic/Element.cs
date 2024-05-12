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
        public List<Classes.Element> GetElementsByType(string type)
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

            var output = GetElements(selectQuery, MainClass.gameDb, parms);
            return output;
        }

        public List<Classes.Element> GetRandOutputElements()
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

            var output = GetElements(selectQuery);
            return output;
        }

        public List<Classes.Element> GetAttribWithReference()
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
    AND Output LIKE '%((%'
    AND ElementType = 'attribute'
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var output = GetElements(selectQuery);
            return output;
        }

        internal bool SetElementParentKey(string key, string parentKey)
        {
            var setKeyOutput = false;

            string connectionString = Connection.GetConnection();
            if (connectionString.Equals(""))
            {
                return setKeyOutput;
            }

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
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
                setKeyOutput = true;
            }

            CacheManager.RefreshCache();

            return setKeyOutput;
        }

        public Classes.Element GetElementByKey(string element_key)
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


            var output = GetElement(selectQuery, parms);

            return output;
        }

        public List<Classes.Element> GetElementChildren(string parent_key)
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


            var output = GetElements(selectQuery, MainClass.gameDb, parms);

            return output;
        }

        public string GetElementField(string element_key, string element_field)
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

            string connectionString = Connection.GetConnection();

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

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return output;
        }


        public Classes.Element GetElement(string selectQuery, List<DbParameter> parms)
        {
            var elementOutput = new Classes.Element();

            string connectionString = Connection.GetConnection();
            if (connectionString.Equals(""))
            {
                return elementOutput;
            }

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
                            e.Children = GetElementChildren(reader.GetString(reader.GetOrdinal("ElementKey")));

                            elementOutput = e;
                            break;
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return elementOutput;
        }

        public List<Classes.Element> GetElements(string selectQuery)
        {
            return GetElements(selectQuery, MainClass.gameDb, new List<DbParameter>());
        }
        public List<Classes.Element> GetElements(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var elementsOutput = new List<Classes.Element>();

            string connectionString = Connection.GetConnection(gameDb);
            if (connectionString.Equals(""))
            {
                return elementsOutput;
            }

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
                            e.Children = GetElementChildren(reader.GetString(reader.GetOrdinal("ElementKey")));

                            elementsOutput.Add(e);
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return elementsOutput;
        }

        public List<Classes.SearchElement> GetSearchElements(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = new List<Classes.SearchElement>();

            var gameFile = $"Games/{gameDb}.db";

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

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return output;
        }

        public bool SetElementField(string element_key, string field, string new_value, bool refreshCache = true)
        {
            var output = false;

            string connectionString = Connection.GetConnection();

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

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
                output = true;
            }

            if (refreshCache)
            {
                CacheManager.RefreshCache();
            }

            return output;
        }

        public int GetRepeatIndex(string element_key)
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


            var output = GetInt(selectQuery, MainClass.gameDb, parms);

            return output;
        }

        private int GetInt(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = 0;

            var gameFile = $"Games/{gameDb}.db";

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

                    connection.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return output;
        }

        internal string ElementLookup(string gameDb, string idValue)
        {
            var elem = GetElementByKey(idValue);
            if (!elem.ElementKey.Equals(""))
            {
                return elem.ElementKey;
            }

            var elems = GetElementKeysBySyntax(idValue);
            if (elems.Count == 1)
            {
                return elems.First();
            }

            return "";
        }

        internal List<string> GetElementKeysBySyntax(string idValue)
        {
            return GetElementKeysBySyntax(MainClass.gameDb, idValue, false);
        }
        internal List<string> GetElementKeysBySyntax(string gameDb, string idValue, bool activeOnly)
        {
            idValue = idValue.Trim();
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

        internal List<string> GetChildElementKeysBySyntax(Classes.Element parentElement, string idValue, bool activeOnly)
        {
            idValue = idValue.Trim();
            var output = new List<string>();
            var allElems = parentElement.Children.Where(c => c.Syntax != "");

            foreach (var elem in allElems)
            {
                Regex rgx = new Regex(elem.Syntax, RegexOptions.IgnoreCase);

                if (rgx.IsMatch(idValue))
                {
                    if (!activeOnly || elem.Active == "true")
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
                        if (!activeOnly || elem.Active == "true")
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
    AND Syntax != ''
;
            ";

            var parms = new List<DbParameter>();

            var output = GetSearchElements(selectQuery, gameDb, parms);

            return output;
        }

        internal void SetRandOutputElements()
        {
            var elems = GetRandOutputElements();
            string connectionString = Connection.GetConnection();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                foreach (var elem in elems)
                {
                    elem.Output = elem.Output.Randomize();

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

                        command.Dispose();
                    }
                }

                connection.Close();
                connection.Dispose();
                Tools.CacheManager.RefreshCache();
            }
        }

        internal void SetAttribReference()
        {
            var elems = GetAttribWithReference();

            foreach (var elem in elems)
            {
                var attrArr = elem.Output.Split("((");
                if (attrArr.Length == 2)
                {
                    var elemKey = attrArr[0].Trim();
                    var elemAttr = attrArr[1].Replace("))", "").Trim();

                    var selElem = GetElementByKey(elemKey);

                    foreach (var elemChild in selElem.Children)
                    {
                        if (elemChild.Tags.TagsContain(elemAttr))
                        {
                            elem.Output = elemChild.Output;
                            SetElementField(elem.ElementKey, "Output", elemChild.Output, true);
                        }
                    }
                }
            }
        }
    }
}
