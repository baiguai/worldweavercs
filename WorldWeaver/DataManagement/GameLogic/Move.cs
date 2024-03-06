using System;
using WorldWeaver.Classes;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Move
    {
        internal Output MoveElement(Output output, string gameDb, string location, string tags, string userInput)
        {
            var tagList = tags.Split('|');
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = new Parsers.Elements.Element();
            var success = false;

            foreach (var tag in tagList)
            {
                success = elemDb.SetElementLocation(gameDb, tag, location);
            }

            if (success)
            {
                var locElem = elemDb.GetElementByKey(gameDb, location);
                Cache.RoomCache.Room = locElem;
                var procItem = new ElementProc();

                procItem.CurrentElementTypes.Add(locElem.ElementType);
                procItem.ChildProcElements.Add("enter_message");
                procItem.AllowRepeatOptions = true;

                output = elem.ParseElement(output, gameDb, locElem, userInput, procItem);
            }

            return output;
        }
    }
}
