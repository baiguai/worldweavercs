using System;
using WorldWeaver.Tools;
namespace WorldWeaver.DataManagement.Game
{
    public class PlayGame
    {
        public void StartGame()
        {
            var gameLogic = new DataManagement.GameLogic.Game();

            var gameKey = gameLogic.GetKey(MainClass.gameDb);

            MainClass.output.Value = gameKey;
            MainClass.output.MatchMade = gameKey.Equals(""); // Set it to return the output if none found
        }

        public void SetPlayerName(string name)
        {
            var elemLogic = new DataManagement.GameLogic.Element();

            if (elemLogic.SetElementField("player", "name", name))
            {
                ProcessCustomStaticValues();
                MainClass.output.OutputText = $"Player name set to {name}.{Environment.NewLine}{Environment.NewLine}";
                MainClass.output.MatchMade = true;
            }
            else
            {
                MainClass.output.OutputText = $"Unable to set the player name.";
                MainClass.output.MatchMade = true;
            }

            CacheManager.RefreshCache();

            return;
        }

        private void ProcessCustomStaticValues()
        {
            ProcessRandomOutput();

            CacheManager.RefreshCache();
        }

        private void ProcessRandomOutput()
        {
            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetRandOutputElements();
        }
    }
}
