namespace WorldWeaver.Tools
{
    public class Game
    {
        public static bool IsDay()
        {
            var gameData = new DataManagement.GameLogic.Game();
            var time = gameData.GetTime();
            var arrTime = time.Split(':');
            var hour = Convert.ToInt32(arrTime[0].Trim());

            if (hour > Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "day_end")) ||
                hour < Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "day_start")))
            {
                return false;
            }

            return true;
        }

        public static int MissionDays()
        {
            var gameData = new DataManagement.GameLogic.Game();
            var days = gameData.GetMissionDays();
            return days;
        }

        public static int TotalDays()
        {
            var gameData = new DataManagement.GameLogic.Game();
            var days = gameData.GetTotalDays();
            return days;
        }

        public static string CurrentTime()
        {
            var gameData = new DataManagement.GameLogic.Game();
            var curTime = gameData.GetTime();
            return curTime;
        }

        public static void IncrementTime()
        {
            var gameData = new DataManagement.GameLogic.Game();
            var curTime = gameData.GetTime();
            var arrTime = curTime.Split(':');
            var hour = Convert.ToInt32(arrTime[0].Trim());
            var min = Convert.ToInt32(arrTime[1].Trim());
            var incr = Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "turn_minutes"));

            min += incr;
            if (min > 59)
            {
                min = 0;
                hour++;
                if (hour > 23)
                {
                    hour = 0;
                }
            }

            gameData.UpdateGameState("TimeHour", hour);
            gameData.UpdateGameState("TimeMinute", min);
        }

        internal static void RemoveInProgressGame()
        {
            var gameFile = $"Games/{MainClass.gameDb}.db";
            File.Delete(gameFile);
            MainClass.gameDb = "";
        }

        internal static void ClearEverything()
        {
            Tools.CacheManager.ClearCache();
            Tools.Game.RemoveInProgressGame();
            MainClass.gameDb = "";
        }
    }
}