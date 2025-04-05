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
            var dropping = false;

            foreach (var tag in tagList)
            {
                var newElem = new Classes.Element();

                newElem = Tools.Elements.GetRelativeElementKey(currentElement, tag);

                if (newElem.Equals(Cache.PlayerCache.Player.ElementKey))
                {
                    MainClass.output.PlayerMoved = true;
                }

                var relElem = Tools.Elements.GetRelativeElementKey(currentElement, newParentKey);
                if (relElem != null)
                {
                    newParentKey = relElem.ElementKey;
                }

                if (newParentKey.Equals(""))
                {
                    return;
                }

                if (newElem.ParentKey.Equals(Cache.PlayerCache.Player.ElementKey, StringComparison.OrdinalIgnoreCase) &&
                    !newParentKey.Equals(Cache.PlayerCache.Player.ElementKey, StringComparison.OrdinalIgnoreCase))
                {
                    dropping = true;
                }

                success = elemDb.SetElementParentKey(newElem.ElementKey, newParentKey);

                if (dropping && newElem.Tags.TagsContain("!_weapon"))
                {
                    var playerWeapon = Cache.PlayerCache.Player.AttributeByTag("armed");
                    if (playerWeapon.ElementKey.Equals(newElem.ElementKey))
                    {
                        var setCls = new Parsers.Elements.Set();
                        setCls.SetDefaultArmed();
                    }
                }

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
