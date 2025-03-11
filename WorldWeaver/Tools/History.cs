
namespace WorldWeaver.Tools
{
    public class History
    {
        public static void AddHistoryItem(string command, int maxHistoryItems)
        {
            if (MainClass.history.Count == maxHistoryItems && !MainClass.history.Contains(command))
            {
                MainClass.history.RemoveAt(MainClass.history.Count-1);
            }

            if (MainClass.history.Contains(command))
            {
                for (var i = 0; i < MainClass.history.Count; i++)
                {
                    if (MainClass.history[i].Equals(command))
                    {
                        MainClass.history.RemoveAt(i);
                        break;
                    }
                }
            }

            MainClass.history.Insert(0, command);
        }

        public static string ListHistory()
        {
            var histOutput = "";

            for (var i = 0; i < MainClass.history.Count; i++)
            {
                if (!histOutput.Equals(""))
                {
                    histOutput = histOutput + Environment.NewLine;
                }
                histOutput += $"({i + 1}) {MainClass.history[i]}";
            }

            return histOutput;
        }

        internal static string GetHistory(string histInput)
        {
            var histOutput = "";

            try
            {
                var index = Convert.ToInt32(histInput) - 1;
                histOutput = MainClass.history[index];
            }
            catch (Exception)
            {
                DataManagement.GameLogic.Element elemDb = new DataManagement.GameLogic.Element();
                var attribs = elemDb.GetElementsByTag("!_error");
                if (attribs == null || attribs.Count < 1)
                {
                    return "";
                }
                else
                {
                    return attribs.First().Output;
                }
            }

            return histOutput;
        }
    }
}