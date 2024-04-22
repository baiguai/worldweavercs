using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public Classes.Output ParseMove(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, int currentIndex, string userInput)
        {
            var moveDb = new DataManagement.GameLogic.Move();

            if (currentElement.Logic.Equals("[self]"))
            {
                currentElement.Logic = Tools.Elements.GetSelf(gameDb, currentElement).ElementKey;
            }

            output = moveDb.MoveElement(output, gameDb, currentElement.Output, currentElement.Tags, currentElement.Logic, userInput);

            return output;
        }
    }
}
