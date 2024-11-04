using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace WorldWeaver.DataManagement
{
    public class ConfigDb
    {
        public string connection = "";

        public ConfigDb()
        {
            connection = Connection.GetConfigConnection();
        }
    }
}
