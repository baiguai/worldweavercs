using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Game
    {
        public string GetKey(string gameDb)
        {
            var output = gameDb;
            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return "";
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            string selectQuery = @"
SELECT
    ElementKey
FROM
    element
WHERE
    ElementType = 'game'
LIMIT 1
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(selectQuery, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            output = reader.GetString(reader.GetOrdinal("ElementKey"));
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public bool IsGameRunning(string gameDb)
        {
            var output = false;

            if (Cache.GameCache.Game != null)
            {
                return true;
            }

            return output;
        }
    }
}
