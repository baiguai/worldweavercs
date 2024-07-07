using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public void ParseMove(Classes.Element parentElement, Classes.Element currentElement, int currentIndex)
        {
            var moveDb = new DataManagement.GameLogic.Move();

            if (currentElement.Logic.Equals("[self]"))
            {
                currentElement.Logic = Tools.Elements.GetSelf(currentElement).ElementKey;
            }

            moveDb.MoveElement(currentElement, currentElement.Output, currentElement.Tags, currentElement.Logic);

            return;
        }
    }
}
