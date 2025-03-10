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
        public List<Classes.Element> elementsToUpdate = new List<Classes.Element>();
        public string previousFieldName = "";
        public int commitSize = 50;

        public List<Classes.Element> GetElementsByType(string type)
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);
            return GetElementsByType(connectionString, type);
        }
        public List<Classes.Element> GetElementsByType(string connectionString, string type)
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

            var output = GetElements(connectionString, selectQuery, parms);
            return output;
        }

        public List<Classes.Element> GetElementsByTag(string tag)
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);

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
    AND '|'||Tags||'|' LIKE '%'||@tag||'%'
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@tag",
                ParamValue = tag
            });

            var output = GetElements(connectionString, selectQuery, parms);
            return output;
        }

        public List<Classes.Element> GetRoomElementsByTag(string tag, string roomKey)
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);

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
    AND '|'||Tags||'|' LIKE '%'||@tag||'%'
    AND ParentKey = @parent
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@tag",
                ParamValue = tag
            });
            parms.Add(new DbParameter()
            {
                ParamName = "@parent",
                ParamValue = roomKey
            });

            var output = GetElements(connectionString, selectQuery, parms);
            return output;
        }

        public List<Classes.Element> GetRandOutputElements()
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);
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
    AND (
        Output LIKE '[rand:%' OR
        Output LIKE '[roll:%'
    )
ORDER BY
    sort
;
            ";

            var output = GetElements(connectionString, selectQuery);
            return output;
        }

        private List<Classes.Element> GetRandNameElements()
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);
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
    AND (
        Name LIKE '%||%'
    )
ORDER BY
    sort
;
            ";

            var output = GetElements(connectionString, selectQuery);
            return output;
        }

        public List<Classes.Element> GetAttribWithReference()
        {
            var connectionString = Connection.GetConnection(MainClass.gameDb);
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
    AND (ElementType = 'attribute' OR ElementType = 'attrib')
    AND Active = 'true'
ORDER BY
    sort
;
            ";

            var output = GetElements(connectionString, selectQuery);
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
            return GetElementByKey("", element_key);
        }
        public Classes.Element GetElementByKey(string connectionString, string element_key)
        {
            if (connectionString.Equals(""))
            {
                connectionString = Connection.GetConnection();
            }
            var cachedElem = CacheManager.GetCachedElement(element_key);
            if (cachedElem != null && !cachedElem.ElementKey.Equals(""))
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


            Classes.Element? output = null;

            if (connectionString.Equals(""))
            {
                output = GetElement(selectQuery, parms);
            }
            else
            {
                output = GetElement(connectionString, selectQuery, parms);
            }

            return output;
        }


        public List<Classes.Element> GetElementChildren(string parent_key, bool includeAttributes = true)
        {
            return GetElementChildren("", parent_key, includeAttributes);
        }

        public List<Classes.Element> GetElementChildren(string connectionString, string parent_key, bool includeAttributes = true)
        {
            var cachedElem = Tools.CacheManager.GetCachedElement(parent_key);
            if (connectionString.Equals(""))
            {
                connectionString = Connection.GetConnection(MainClass.gameDb);
            }

            if (cachedElem != null)
            {
                if (!includeAttributes)
                {
                    return cachedElem.Children.Where(c => c.ElementType != "attribute" && c.ElementType != "attrib").ToList();
                }
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


            var output = GetElements(connectionString, selectQuery, parms);
            if (!includeAttributes)
            {
                output = output.Where(c => c.ElementType != "attribute" && c.ElementType != "attrib").ToList();
            }

            return output;
        }

        public List<Classes.Element> GetElementChildrenByType(string parent_key, string childType)
        {
            var cachedElem = Tools.CacheManager.GetCachedElement(parent_key);
            string connectionString = Connection.GetConnection(MainClass.gameDb);
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
    AND ElementType = @childtype
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
            parms.Add(new DbParameter()
            {
                ParamName = "@childtype",
                ParamValue = childType
            });

            var output = GetElements(connectionString, selectQuery, parms);

            return output;
        }
        public List<Classes.Element> GetDoors(string parent_key)
        {
            var cachedElem = Tools.CacheManager.GetCachedElement(parent_key);
            var connectionString = Connection.GetConnection(MainClass.gameDb);
            if (cachedElem != null)
            {
                return cachedElem.Children;
            }

            var selectQuery = $@"
SELECT
    mv.ElementType,
    mv.ElementKey,
    mv.Name,
    mv.ParentKey,
    mv.Syntax,
    mv.Logic,
    mv.Output,
    mv.Tags,
    mv.Repeat,
    mv.RepeatIndex,
    mv.Active,
    mv.Sort
FROM
    element inp
    INNER JOIN element mv
         ON mv.ParentKey = inp.ElementKey
WHERE 1=1
    AND inp.ParentKey = @parentkey
    AND inp.Active = 'true'
    AND inp.ElementType = 'input'
    AND mv.ElementType = 'move'
    AND mv.Active = 'true'
    AND mv.Tags LIKE '%[player]%'
ORDER BY
    inp.Sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@parentkey",
                ParamValue = parent_key
            });

            var output = GetElements(connectionString, selectQuery, parms);

            foreach (var door in output)
            {
                var parent = GetElementByKey(door.ParentKey);
                door.Syntax = parent.Syntax;
            }

            return output;
        }

        public Classes.Element GetElementChildByTag(string parent_key, string tag)
        {
            var cachedElem = Tools.CacheManager.GetCachedElement(parent_key);
            var connectionString = Connection.GetConnection(MainClass.gameDb);
            if (cachedElem != null && cachedElem.Children.Count > 0)
            {
                var cachedChild = cachedElem.Children.Where(c => c.Tags.TagsContain(tag)).FirstOrDefault();
                if (cachedChild != null)
                {
                    return cachedChild;
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


            var output = GetElements(connectionString, selectQuery, parms).Where(c => c.Tags.TagsContain(tag)).First();

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
            string connectionString = Connection.GetConnection();
            return GetElement(connectionString, selectQuery, parms);
        }
        public Classes.Element GetElement(string connectionString, string selectQuery, List<DbParameter> parms)
        {
            var elementOutput = new Classes.Element();

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
                            e.Children = GetElementChildren(connectionString, reader.GetString(reader.GetOrdinal("ElementKey")));

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

        public List<Classes.Element> GetElements(string connectionString, string selectQuery)
        {
            return GetElements(connectionString, selectQuery, new List<DbParameter>());
        }
        public List<Classes.Element> GetElements(string connectionString, string selectQuery, List<DbParameter> parms)
        {
            var elementsOutput = new List<Classes.Element>();

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
                            e.Children = GetElementChildren(connectionString, reader.GetString(reader.GetOrdinal("ElementKey")));

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
            var success = false;

            field = Tools.Elements.FixPropertyName(field);

            if (field.Equals("tags", StringComparison.CurrentCultureIgnoreCase))
            {
                new_value = Tools.Elements.FixTagsUpdateValue(element_key, new_value);
            }

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
                success = true;
            }

            if (refreshCache)
            {
                CacheManager.RefreshCache();
            }

            return success;
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

            foreach (var elem in elems)
            {
                elem.Output = elem.Output.RandomValue(elem);
                AddUpdateElement(connectionString, elem, "Output");
            }
            UpdateElements(connectionString, "Output");
        }

        internal void ApplyRandOutputElements(string connectionString, string sql)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var primContainers = AppSettingFunctions.GetRootArray("Config/PrimaryContainers.json");

                using (SqliteCommand command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
                Tools.CacheManager.RefreshCache();
            }
        }

        internal void SetAttribReference()
        {
            var elems = GetAttribWithReference();
            string connectionString = Connection.GetConnection();

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
                            AddUpdateElement(connectionString, elemChild, "Output");
                        }
                    }
                }
            }
            UpdateElements(connectionString, "Output");
        }

        internal void SetRandNameElements()
        {
            var elems = GetRandNameElements();
            string connectionString = Connection.GetConnection();

            foreach (var elem in elems)
            {
                elem.Name = elem.Name.RandomSplitValue();
                AddUpdateElement(connectionString, elem, "Name");
            }
            UpdateElements(connectionString, "Name");
        }

        internal string SpawnTemplateElement(Classes.Element currentElement, string templateKey, string tags)
        {
            DataManagement.Game.BuildGame gameDb = new DataManagement.Game.BuildGame();
            DataManagement.GameLogic.Element elemDb = new DataManagement.GameLogic.Element();
            var key = Guid.NewGuid().ToString();

            var tmpltElement = elemDb.GetElementByKey(templateKey);

            var parentAttr = tmpltElement.AttributeByTag("parent");
            if (parentAttr == null)
            {
                return "";
            }
            var pKey = Tools.Elements.GetRelativeElement(currentElement, parentAttr.Output).ElementKey;
            tmpltElement.ParentKey = pKey;
            tmpltElement.Name = Tools.Template.GetTemplateName(currentElement, tmpltElement.Name);
            tmpltElement.ElementKey = key;
            tmpltElement.Tags.AddTag(tags);
            if (tmpltElement.Tags.Equals(""))
            {
                tmpltElement.Tags = tags;
            }

            gameDb.SaveElement(tmpltElement);

            foreach (var child in tmpltElement.Children)
            {
                SpawnChildElement(tmpltElement, child);
            }

            return key;
        }

        internal void SpawnChildElement(Classes.Element parentElement, Classes.Element childElement)
        {
            DataManagement.Game.BuildGame gameDb = new DataManagement.Game.BuildGame();
            childElement.ParentKey = parentElement.ElementKey;
            childElement.ElementKey = Guid.NewGuid().ToString();
            gameDb.SaveElement(childElement);

            foreach (var child in childElement.Children)
            {
                SpawnChildElement(childElement, child);
            }
        }

        internal bool AddNote(string noteKey, string noteText)
        {
            var success = false;

            if (NoteKeyExists(noteKey))
            {
                MainClass.output.OutputText = "Note key already exists. Note keys must be unique.";
                MainClass.output.MatchMade = true;
                return success;
            }

            string connectionString = Connection.GetConnection();

            var updateQuery = $@"
INSERT INTO
    note ( NoteKey, NoteText )
Values (
    @noteKey,
    @noteText
);
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@noteKey", noteKey);
                    command.Parameters.AddWithValue("@noteText", noteText);

                    command.ExecuteNonQuery();

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
                success = true;
            }

            success = true;
            MainClass.output.OutputText = "Note successfully added.";
            MainClass.output.MatchMade = true;

            return success;
        }

        internal bool DeleteNote(string noteKey)
        {
            var success = false;

            if (!NoteKeyExists(noteKey))
            {
                MainClass.output.OutputText = "Note key doesn't exist. Specify an existing note key.";
                MainClass.output.MatchMade = true;
                return success;
            }

            string connectionString = Connection.GetConnection();

            var updateQuery = $@"
DELETE FROM
    note
WHERE 1=1
    AND NoteKey = @noteKey
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@noteKey", noteKey);

                    command.ExecuteNonQuery();

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
                success = true;
            }

            success = true;
            MainClass.output.OutputText = "Note successfully deleted.";
            MainClass.output.MatchMade = true;

            return success;
        }

        internal bool NoteKeyExists(string noteKey)
        {
            var exists = false;

            string connectionString = Connection.GetConnection();

            var updateQuery = $@"
SELECT
    COUNT(*) AS NoteCount
FROM
    note
WHERE 1=1
    AND NoteKey = @notekey
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@notekey", noteKey);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var noteCount = reader.GetInt32(reader.GetOrdinal("NoteCount"));
                            if (noteCount > 0)
                            {
                                exists = true;
                            }
                            break;
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return exists;
        }

        internal List<string> ListNotes()
        {
            var notesList = new List<string>();
            string connectionString = Connection.GetConnection();

            var updateQuery = $@"
SELECT
    NoteKey
FROM
    note
Order By
    NoteKey
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var lbl = reader.GetString(reader.GetOrdinal("NoteKey"));
                            if (!lbl.Equals(""))
                            {
                                notesList.Add(lbl);
                            }
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return notesList;
        }

        internal string ViewNote(string noteKey)
        {
            var noteTxt = "";
            string connectionString = Connection.GetConnection();

            var updateQuery = $@"
SELECT
    NoteText
FROM
    note
WHERE
    NoteKey = @notekey
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@notekey", noteKey);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            noteTxt = reader.GetString(reader.GetOrdinal("NoteText"));
                            break;
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return noteTxt;
        }

        internal List<NoteSearch> SearchAdminNotes(string searchString)
        {
            var notes = GetElementsByType("devnote");

            var found = notes.Where(n => n.Tags.Contains(searchString)).ToList();
            found.AddRange(notes.Where(n => n.Output.Contains(searchString)).ToList());

            List<NoteSearch> srchOutput = new List<NoteSearch>();

            foreach (var itm in found)
            {
                var parent = GetElementByKey(itm.ParentKey);
                var i = new NoteSearch() {
                    Parent = parent.Name,
                    Output = itm.Output
                };
                srchOutput.Add(i);
            }

            return srchOutput;
        }



        private bool UpdateElements(string connectionString, string fieldName)
        {
            if (elementsToUpdate == null || elementsToUpdate.Count < 1)
            {
                return true;
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var caseItms = "";
                var whereItms = "";
                var newValue = "";

                if (elementsToUpdate == null || elementsToUpdate.Count < 1)
                {
                    return true;
                }

                foreach (var e in elementsToUpdate)
                {
                    newValue = GetElementFieldValue(e, fieldName);
                    caseItms += $"   WHEN ElementKey = '{e.ElementKey}' THEN '{newValue}'{Environment.NewLine}";
                    if (!whereItms.Equals(""))
                    {
                        whereItms += ", ";
                    }
                    whereItms += $"'{e.ElementKey}'";

                }

                var createDbQuery = $@"
UPDATE element
SET {fieldName} =
CASE
    {caseItms}
    ELSE {fieldName}
END
WHERE
    ElementKey IN ({whereItms})
;
";

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

                elementsToUpdate.Clear();
                connection.Close();
                connection.Dispose();
                Tools.CacheManager.RefreshCache();
                return true;
            }
        }

        private string GetElementFieldValue(Classes.Element e, string fieldName)
        {
            switch (fieldName)
            {
                case "Name":
                    return e.Name;

                case "Output":
                    return e.Output;

                default:
                    return "";
            }
        }

        private bool AddUpdateElement(string connectionString, Classes.Element element, string fieldName)
        {
            var success = false;

            if (!fieldName.Equals(previousFieldName))
            {
                UpdateElements(connectionString, previousFieldName);
            }

            previousFieldName = fieldName;

            if (elementsToUpdate.Count() > commitSize)
            {
                success = UpdateElements(connectionString, fieldName);
            }
            else
            {
                elementsToUpdate.Add(element);
            }

            return success;
        }
    }
}
