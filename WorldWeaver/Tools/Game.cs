namespace WorldWeaver.Tools
{
    public class Game
    {
        public static bool IsDay(string gameDb)
        {
            var gameData = new DataManagement.GameLogic.Game();
            var time = gameData.GetTime(gameDb);
            var arrTime = time.Split(':');
            var hour = Convert.ToInt32(arrTime[0].Trim());

            if (hour > Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "day_end")) ||
                hour < Convert.ToInt32(Tools.AppSettingFunctions.GetConfigValue("time", "day_start")))
            {
                return false;
            }

            return true;
        }

        public static int MissionDays(string gameDb)
        {
            var gameData = new DataManagement.GameLogic.Game();
            var days = gameData.GetMissionDays(gameDb);
            return days;
        }

        public static int TotalDays(string gameDb)
        {
            var gameData = new DataManagement.GameLogic.Game();
            var days = gameData.GetTotalDays(gameDb);
            return days;
        }

        public static string CurrentTime(string gameDb)
        {
            var gameData = new DataManagement.GameLogic.Game();
            var curTime = gameData.GetTime(gameDb);
            return curTime;
        }

        public static void IncrementTime(string gameDb)
        {
            var gameData = new DataManagement.GameLogic.Game();
            var curTime = gameData.GetTime(gameDb);
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

            gameData.UpdateGameState(gameDb, "TimeHour", hour);
            gameData.UpdateGameState(gameDb, "TimeMinute", min);
        }
    }
}