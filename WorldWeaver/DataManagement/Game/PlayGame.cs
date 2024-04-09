using System;
using WorldWeaver.Tools;
namespace WorldWeaver.DataManagement.Game
{
    public class PlayGame
    {
        public Classes.Output StartGame(Classes.Output output, string gameDb)
        {
            var gameLogic = new DataManagement.GameLogic.Game();

            var gameKey = gameLogic.GetKey(gameDb);

            output.Value = gameKey;
            output.MatchMade = gameKey.Equals(""); // Set it to return the output if none found

            return output;
        }

        public Classes.Output SetPlayerName(Classes.Output output, string gameDb, string name)
        {
            var elemLogic = new DataManagement.GameLogic.Element();

            if (elemLogic.SetElementField(gameDb, "player", "name", name))
            {
                ProcessCustomStaticValues(gameDb);
                output.OutputText = $"Player name set to {name}.{Environment.NewLine}{Environment.NewLine}";
                output.MatchMade = true;
            }
            else
            {
                output.OutputText = $"Unable to set the player name.";
                output.MatchMade = true;
            }

            CacheManager.RefreshCache(gameDb);

            return output;
        }

        private void ProcessCustomStaticValues(string gameDb)
        {
            ProcessRandomOutput(gameDb);

            CacheManager.RefreshCache(gameDb);
        }

        private void ProcessRandomOutput(string gameDb)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            elemDb.SetRandOutputElements(gameDb);
        }
    }
}
