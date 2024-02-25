using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Move
    {
        public Classes.Output ParseMove(string gameDb, Classes.Element moveElement)
        {
            var output = new Classes.Output();
            var moveDb = new DataManagement.GameLogic.Move();
            var procSteps = Tools.ProcFunctions.GetProcessStepsByType(moveElement.element_type);

            output.OutputText = moveElement.output;
            output.MatchMade = true;

            return output;
        }
    }
}
