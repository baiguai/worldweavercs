using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class DevNote
    {
        public void ParseDevNote(Classes.Element noteElement)
        {
            MainClass.output.OutputText += $@"
                

Type: {noteElement.Tags}
Note: {noteElement.Output}
                ";
        }
    }
}
