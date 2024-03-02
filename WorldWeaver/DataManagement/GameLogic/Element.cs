using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.Sqlite;
using WorldWeaver.Classes;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Element
    {
        public List<Classes.Element> GetElementsByType(string gameDb, string type)
        {
            var selectQuery = $@"
SELECT
    element_type,
    element_key,
    name,
    parent_key,
    location,
    syntax,
    logic,
    output,
    tags,
    repeat_type,
    repeat_index,
    active,
    sort
FROM
    element
WHERE 1=1
    AND element_type = @type
    AND active = '1'
ORDER BY
    sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                param_name = "@type",
                param_value = type
            });

            var output = GetElements(selectQuery, gameDb, parms);
            return output;
        }

        internal bool SetElementLocation(string gameDb, string key, string location)
        {
            var output = false;

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Version=3;";

            var updateQuery = $@"
UPDATE
    element
SET
    location = @newlocation
WHERE 1=1
    AND element_key = @elementkey
    AND active = '1'
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@elementkey", key);
                    command.Parameters.AddWithValue("@newlocation", location);

                    command.ExecuteNonQuery();
                }

                connection.Close();
                output = true;
            }

            return output;
        }

        public Classes.Element GetElementByKey(string gameDb, string element_key)
        {
            var selectQuery = $@"
SELECT
    element_type,
    element_key,
    name,
    parent_key,
    location,
    syntax,
    logic,
    output,
    tags,
    repeat_type,
    repeat_index,
    active,
    sort
FROM
    element
WHERE 1=1
    AND element_key = @elementkey
    AND active = '1'
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                param_name = "@elementkey",
                param_value = element_key
            });


            var output = GetElement(selectQuery, gameDb, parms);

            return output;
        }

        public List<Classes.Element> GetElementChildren(string gameDb, string parent_key)
        {
            var selectQuery = $@"
SELECT
    element_type,
    element_key,
    name,
    parent_key,
    location,
    syntax,
    logic,
    output,
    tags,
    repeat_type,
    repeat_index,
    active,
    sort
FROM
    element
WHERE 1=1
    AND parent_key = @parentkey
    AND active = '1'
ORDER BY
    sort
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            { 
                param_name = "@parentkey",
                param_value = parent_key
            });


            var output = GetElements(selectQuery, gameDb, parms);

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

            string connectionString = $"Data Source={gameFile};Version=3;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.param_name, parm.param_value);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var e = new Classes.Element();

                            e.element_type = reader.GetString(reader.GetOrdinal("element_type"));
                            e.element_key = reader.GetString(reader.GetOrdinal("element_key"));
                            if (reader["name"] != DBNull.Value) { e.name = reader.GetString(reader.GetOrdinal("name")); }
                            e.parent_key = reader.GetString(reader.GetOrdinal("parent_key"));
                            e.location = reader.GetString(reader.GetOrdinal("location"));
                            if (reader["syntax"] != DBNull.Value) { e.syntax = reader.GetString(reader.GetOrdinal("syntax")); }
                            if (reader["logic"] != DBNull.Value) { e.logic = reader.GetString(reader.GetOrdinal("logic")); }
                            if (reader["output"] != DBNull.Value) { e.output = reader.GetString(reader.GetOrdinal("output")); }
                            if (reader["tags"] != DBNull.Value) { e.tags = reader.GetString(reader.GetOrdinal("tags")); }
                            e.repeat = reader.GetString(reader.GetOrdinal("repeat_type"));
                            e.repeat_index = reader.GetInt32(reader.GetOrdinal("repeat_index"));
                            e.active = reader.GetString(reader.GetOrdinal("active"));
                            e.sort = reader.GetInt32(reader.GetOrdinal("sort"));
                            e.children = GetElementChildren(gameDb, reader.GetString(reader.GetOrdinal("element_key")));

                            output = e;
                            break;
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public List<Classes.Element> GetElements(string selectQuery, string gameDb, List<DbParameter> parms)
        {
            var output = new List<Classes.Element>();

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Version=3;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.param_name, parm.param_value);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var e = new Classes.Element();

                            e.element_type = reader.GetString(reader.GetOrdinal("element_type"));
                            e.element_key = reader.GetString(reader.GetOrdinal("element_key"));
                            if (reader["name"] != DBNull.Value) { e.name = reader.GetString(reader.GetOrdinal("name")); }
                            e.parent_key = reader.GetString(reader.GetOrdinal("parent_key"));
                            e.location = reader.GetString(reader.GetOrdinal("location"));
                            if (reader["syntax"] != DBNull.Value) { e.syntax = reader.GetString(reader.GetOrdinal("syntax")); }
                            if (reader["logic"] != DBNull.Value) { e.logic = reader.GetString(reader.GetOrdinal("logic")); }
                            if (reader["output"] != DBNull.Value) { e.output = reader.GetString(reader.GetOrdinal("output")); }
                            if (reader["tags"] != DBNull.Value) { e.tags = reader.GetString(reader.GetOrdinal("tags")); }
                            e.repeat = reader.GetString(reader.GetOrdinal("repeat_type"));
                            e.repeat_index = reader.GetInt32(reader.GetOrdinal("repeat_index"));
                            e.active = reader.GetString(reader.GetOrdinal("active"));
                            e.sort = reader.GetInt32(reader.GetOrdinal("sort"));
                            e.children = GetElementChildren(gameDb, reader.GetString(reader.GetOrdinal("element_key")));

                            output.Add(e);
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public bool SetElementField(string gameDb, string element_key, string field, string new_value)
        {
            var output = false;

            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return output;
            }

            string connectionString = $"Data Source={gameFile};Version=3;";

            var updateQuery = $@"
UPDATE
    element
SET
    {field} = @newvalue
WHERE 1=1
    AND element_key = @elementkey
    AND active = '1'
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

            return output;
        }

        public int GetRepeatIndex(string gameDb, string element_key)
        {
            var selectQuery = $@"
SELECT
    repeat_index
FROM
    element
WHERE 1=1
    AND element_key = @elementkey
    AND active = '1'
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                param_name = "@elementkey",
                param_value = element_key
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

            string connectionString = $"Data Source={gameFile};Version=3;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    foreach (var parm in parms)
                    {
                        command.Parameters.AddWithValue(parm.param_name, parm.param_value);
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
    }
}
