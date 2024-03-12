using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public Classes.Output ParseMove(Classes.Output output, string gameDb, Classes.Element parentElement, Classes.Element currentElement, bool allowRepeatOptions, int currentIndex, string userInput)
        {
            var moveDb = new DataManagement.GameLogic.Move();

            if (!allowRepeatOptions)
            {
                output = moveDb.MoveElement(output, gameDb, currentElement.Output, currentElement.Tags, currentElement.Logic, userInput);
            }
            else
            {
                var moves = Tools.Elements.GetElementsByType(parentElement, "move");
                var idx = 0;
                foreach (var mv in moves)
                {
                    if (idx == currentIndex)
                    {
                        output = moveDb.MoveElement(output, gameDb, mv.Output, currentElement.Tags, currentElement.Logic, userInput);
                        break;
                    }
                    else
                    {
                        idx++;
                    }
                }
            }

            return output;
        }
    }
}
