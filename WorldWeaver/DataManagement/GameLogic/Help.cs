using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Help
    {
        public List<Classes.Help> GetHelpArticles(string inputString)
        {
            string connectionString = Connection.GetConnection(MainClass.gameDb);
            List<Classes.Help> topics = GetHelpTopicsByTitle(connectionString, inputString);

            if (topics.Count() != 1)
            {
                topics = SearchHelpTopics(connectionString, inputString);
            }

            return topics;
        }

        private List<Classes.Help> GetHelpTopicsByTitle(string connectionString, string inputString)
        {
            List<Classes.Help> helpOutput = new List<Classes.Help>();

            if (inputString.StartsWith("?"))
            {
                inputString = inputString.Substring(1).Trim();
            }
            if (inputString.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                inputString = inputString.Substring(4).Trim();
            }

            var selectQuery = $@"
SELECT
    TopicId,
    Title,
    Tags,
    Related,
    Article
FROM
    helpsys
WHERE 1=1
    AND LOWER(Title) = @title
ORDER BY
    Title
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@title",
                ParamValue = inputString.ToLower()
            });

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
                            var h = new Classes.Help();

                            h.TopicId = reader.GetString(reader.GetOrdinal("TopicId"));
                            h.Title = reader.GetString(reader.GetOrdinal("Title"));
                            h.Tags = reader.GetString(reader.GetOrdinal("Tags")).Split('|').ToList();
                            h.Related = reader.GetString(reader.GetOrdinal("Related")).Split('|').ToList();
                            h.Article = reader.GetString(reader.GetOrdinal("Article"));
                            
                            helpOutput.Add(h);
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return helpOutput;
        }

        private List<Classes.Help> SearchHelpTopics(string connectionString, string inputString)
        {
            List<Classes.Help> helpOutput = new List<Classes.Help>();

            if (inputString.StartsWith("?"))
            {
                inputString = inputString.Substring(1).Trim();
            }
            if (inputString.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                inputString = inputString.Substring(4).Trim();
            }

            var selectQuery = $@"
SELECT
    TopicId,
    Title,
    Tags,
    Related,
    Article
FROM
    helpsys
WHERE 1=1
    AND (
        LOWER(Title) LIKE '%'||@input||'%'
        OR
        LOWER(Tags) LIKE '%'||@input||'%'
        OR
        LOWER(Article) LIKE '%'||@input||'%'
    )

ORDER BY
    Title
;
            ";

            var parms = new List<DbParameter>();
            parms.Add(new DbParameter()
            {
                ParamName = "@input",
                ParamValue = inputString.ToLower()
            });

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
                            var h = new Classes.Help();

                            h.TopicId = reader.GetString(reader.GetOrdinal("TopicId"));
                            h.Title = reader.GetString(reader.GetOrdinal("Title"));
                            h.Tags = reader.GetString(reader.GetOrdinal("Tags")).Split('|').ToList();
                            h.Related = reader.GetString(reader.GetOrdinal("Related")).Split('|').ToList();
                            h.Article = reader.GetString(reader.GetOrdinal("Article"));
                            
                            helpOutput.Add(h);
                        }
                    }

                    command.Dispose();
                }

                connection.Close();
                connection.Dispose();
            }

            return helpOutput;
        }
    }
}
