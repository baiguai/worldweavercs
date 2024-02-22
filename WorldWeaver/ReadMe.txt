TOOLS:
SQLiteStudio






SELECT Snippet:

using System;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        // Connection string for your SQLite database
        string connectionString = "Data Source=your_database_file.db;Version=3;";

        // Player name you want to retrieve
        string playerNameToRetrieve = "JohnDoe";

        // SQL query to select a single row based on PlayerName
        string query = "SELECT * FROM Player WHERE PlayerName = @PlayerName";

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                // Add parameter for PlayerName to prevent SQL injection
                command.Parameters.AddWithValue("@PlayerName", playerNameToRetrieve);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Access data from the selected row
                        int playerId = reader.GetInt32(reader.GetOrdinal("PlayerId")); // Assuming "PlayerId" is the column name
                        string playerName = reader.GetString(reader.GetOrdinal("PlayerName"));

                        // Use the retrieved data as needed
                        Console.WriteLine($"Player ID: {playerId}, Player Name: {playerName}");
                    }
                    else
                    {
                        Console.WriteLine("Player not found.");
                    }
                }
            }
        }
    }
}