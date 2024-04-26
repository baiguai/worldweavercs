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

        public string GetTime()
        {
            var timeOutput = "";
            var gameFile = $"Games/{MainClass.gameDb}";
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
                            timeOutput = $"{hour.ToString().PadLeft(2, '0')}:{min.ToString().PadLeft(2, '0')}";
                        }
                    }
                }

                connection.Close();
            }

            return timeOutput;
        }

        public void UpdateGameState(string field, string value)
        {
            var gameFile = $"Games/{MainClass.gameDb}";

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

        public void UpdateGameState(string field, int value)
        {
            var gameFile = $"Games/{MainClass.gameDb}";

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

        internal int GetMissionDays()
        {
            var missionOutput = 0;
            var gameFile = $"Games/{MainClass.gameDb}";

            if (!File.Exists(gameFile))
            {
                return -1;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            string selectQuery = @"
SELECT
    MissionDays
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
                            missionOutput = reader.GetInt32(reader.GetOrdinal("MissionDays"));
                        }
                    }
                }

                connection.Close();
            }

            return missionOutput;
        }

        internal int GetTotalDays()
        {
            var totalOutput = 0;
            var gameFile = $"Games/{MainClass.gameDb}";

            if (!File.Exists(gameFile)) // @todo Create a ConnectionString function
            {
                return -1;
            }

            string connectionString = $"Data Source={gameFile};Cache=Shared;";

            string selectQuery = @"
SELECT
    TotalDays
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
                            totalOutput = reader.GetInt32(reader.GetOrdinal("TotalDays"));
                        }
                    }
                }

                connection.Close();
            }

            return totalOutput;
        }
    }
}
