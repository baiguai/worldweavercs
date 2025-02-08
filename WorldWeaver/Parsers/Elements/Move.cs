using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public void ParseMove(Classes.Element parentElement, Classes.Element currentElement, int currentIndex)
        {
            var moveDb = new DataManagement.GameLogic.Move();
            var logic = Tools.OutputProcessor.ProcessSpecialValue(currentElement, currentElement.Logic);
            if (logic.Equals(""))
            {
                return;
            }

            moveDb.MoveElement(currentElement, currentElement.Tags, logic);

            return;
        }
    }
}
