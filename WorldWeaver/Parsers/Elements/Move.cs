using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public void ParseMove(Classes.Element parentElement, Classes.Element currentElement, int currentIndex)
        {
            var moveDb = new DataManagement.GameLogic.Move();
            var logic = Tools.OutputProcessor.GetNewValue(currentElement, currentElement.Logic);
            if (logic.Equals(""))
            {
                return;
            }
            var moveOutput = Tools.OutputProcessor.GetNewValue(currentElement, currentElement.Output);

            moveDb.MoveElement(currentElement, moveOutput, currentElement.Tags, logic);

            return;
        }
    }
}
