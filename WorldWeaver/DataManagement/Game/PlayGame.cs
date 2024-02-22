using System;
namespace WorldWeaver.DataManagement.Game
{
    public class PlayGame
    {
        public Classes.Output StartGame(Classes.Output output, string gameDb)
        {
            var gameLogic = new DataManagement.GameLogic.Game();

            var gameKey = gameLogic.GetKey(gameDb);

            output.OutputText = gameKey;
            output.MatchMade = gameKey.Equals(""); // Set it to return the output if none found

            return output;
        }

        public Classes.Output SetPlayerName(Classes.Output output, string gameDb, string name)
        {
            var elemLogic = new DataManagement.GameLogic.Element();

            if (elemLogic.SetElementField(gameDb, "player", "name", name))
            {
                output.OutputText = $"Player name set to {name}";
                output.MatchMade = true;
            }
            else
            {
                output.OutputText = $"Unable to set the player name.";
                output.MatchMade = true;
            }

            return output;
        }
    }
}
