using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement
{
    public class Connection
    {
        public static string GetConnection()
        {
            return GetConnection(MainClass.gameDb);
        }
        public static string GetConnection(string gameKey)
        {
            var gameFile = $"{Environment.CurrentDirectory}/Games/{gameKey.FileSafe()}.db";

            if (!File.Exists(gameFile))
            {
                return "";
            }

            return $"Data Source={gameFile};Cache=Shared;";
        }

        public static string GetConfigConnection()
        {
            var configDb = $"{Environment.CurrentDirectory}/Config/config.db";

            return $"Data Source={configDb};Cache=Shared;";
        }
    }
}