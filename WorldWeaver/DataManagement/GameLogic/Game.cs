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

        public static bool IsGameRunning()
        {
            var output = false;

            if (Cache.GameCache.Game != null)
            {
                return true;
            }

            return output;
        }

        public string GetTime(string gameDb)
        {
            var output = "";
            var gameFile = $"Games/{gameDb}";
            var hour = 0;
            var min = 0;

            if (!File.Exists(gameFile))
            {
                return "";
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            string selectQuery = @"
SELECT
    TimeHour,
    TimeMinute
FROM
    gamestate
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
                            hour = reader.GetInt32(reader.GetOrdinal("TimeHour"));
                            min = reader.GetInt32(reader.GetOrdinal("TimeMinute"));
                            output = $"{hour.ToString().PadLeft(2, '0')}:{min.ToString().PadLeft(2, '0')}";
                        }
                    }
                }

                connection.Close();
            }

            return output;
        }

        public void UpdateGameState(string gameDb, string field, string value)
        {
            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            var updateQuery = $@"
UPDATE
    gamestate
SET
    {field} = @newValue
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@newValue", value);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public void UpdateGameState(string gameDb, string field, int value)
        {
            var gameFile = $"Games/{gameDb}";

            if (!File.Exists(gameFile))
            {
                return;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            var updateQuery = $@"
UPDATE
    gamestate
SET
    {field} = @newValue
;
            ";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@newValue", value);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
