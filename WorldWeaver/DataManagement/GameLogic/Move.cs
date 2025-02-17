using System;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.DataManagement.GameLogic
{
    public class Move
    {
        internal void MoveElement(Classes.Element currentElement, string subject, string newParentKey)
        {
            var tagList = subject.Split('|');
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = new Parsers.Elements.Element();
            var success = false;

            foreach (var tag in tagList)
            {
                var key = tag;

                key = Tools.Elements.GetRelativeElementKey(currentElement, key, key);

                if (key.Equals(Cache.PlayerCache.Player.ElementKey))
                {
                    MainClass.output.PlayerMoved = true;
                }

                newParentKey = Tools.Elements.GetRelativeElementKey(currentElement, newParentKey, newParentKey);

                success = elemDb.SetElementParentKey(key, newParentKey);

                var moveOutput = Tools.OutputProcessor.ProcessOutputText(currentElement.Output, currentElement);

                if (success)
                {
                    Tools.CacheManager.RefreshCache();
                    MainClass.output.OutputText += moveOutput;
                    Cache.RoomCache.Room.ParseElement(true);
                }
            }

            return;
        }
    }
}
