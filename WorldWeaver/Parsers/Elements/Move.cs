using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public void ParseMove(Classes.Element parentElement, Classes.Element currentElement, int currentIndex)
        {
            var moveDb = new DataManagement.GameLogic.Move();
            var logic = Tools.OutputProcessor.GetNewValue(currentElement, currentElement.Logic);

            moveDb.MoveElement(currentElement, currentElement.Output, currentElement.Tags, logic);

            return;
        }
    }
}
