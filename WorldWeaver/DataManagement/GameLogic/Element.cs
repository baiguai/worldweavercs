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
    syntax,
    logic,
    output,
    tags,
    active,
    sort
FROM
    element
WHERE
    element_type = @type
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
    active,
    sort
FROM
    element
WHERE
    element_key = @elementkey
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                param_name = "@elementkey",
                param_value = element_key
            });


            var output = GetElement(selectQuery, gameDb, parms);
            foreach (var el in GetElementChildren(gameDb, output.element_key))
            {
                output.children.Add(el);
            }

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
    syntax,
    logic,
    output,
    tags,
    active,
    sort
FROM
    element
WHERE
    parent_key = @parentkey
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
            foreach (var el in output)
            {
                el.children = GetElementChildren(gameDb, el.element_key);
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
                            if (reader["syntax"] != DBNull.Value) { e.syntax = reader.GetString(reader.GetOrdinal("syntax")); }
                            if (reader["logic"] != DBNull.Value) { e.logic = reader.GetString(reader.GetOrdinal("logic")); }
                            if (reader["output"] != DBNull.Value) { e.output = reader.GetString(reader.GetOrdinal("output")); }
                            if (reader["tags"] != DBNull.Value) { e.tags = reader.GetString(reader.GetOrdinal("tags")); }
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
WHERE
    element_key = @elementkey
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
    }
}
