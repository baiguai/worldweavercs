using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorldWeaver.Tools
{
    public class Logger
    {
        public DateTime LogDate { get; set; }
        public enum LogTypes
        {
            BuildGame,
            Error,
            Message
        }
       public void WriteToLog(string? message, LogTypes logType) 
       {
            var typeString = "";

            switch (logType)
            {
                case LogTypes.BuildGame:
                    typeString = "buildgame";
                    break;
                case LogTypes.Error:
                    typeString = "error";
                    break;
                default:
                    typeString = "message";
                    break;
            }

            var path = Tools.AppSettingFunctions.GetConfigValue("logging", "log_path");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += $"{LogDate:MMddyyyyHHmmss}_{typeString}.log";

            File.AppendAllText(path, $"{message}{Environment.NewLine}");
       }
    }
}