using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class DevNote 
    {
        public void ParseDevNote(Classes.Element noteElement)
        {
            if (noteElement.ParentKey.Equals(Cache.RoomCache.Room.ElementKey))
            {
                MainClass.output.OutputText += $@"
                

Type: {noteElement.Tags}
Note: {noteElement.Output}
                ";
                MainClass.output.MatchMade = true;
            }
        }
    }
}
