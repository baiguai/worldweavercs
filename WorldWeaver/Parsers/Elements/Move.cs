using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public Classes.Output ParseMove(Classes.Output output, string gameDb, Classes.Element moveElement, string userInput)
        {
            var moveDb = new DataManagement.GameLogic.Move();

            output = moveDb.MoveElement(output, gameDb, moveElement.location, moveElement.tags, userInput);

            return output;
        }
    }
}
