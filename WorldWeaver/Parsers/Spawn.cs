using System;
using System.Formats.Asn1;
using System.Globalization;
using WorldWeaver.Cache;
using WorldWeaver.Classes;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Spawn
    {
        public string SpawnByTemplate(Classes.Element currentElement)
        {
            DataManagement.GameLogic.Element elemDb = new DataManagement.GameLogic.Element();

            var tmplElem = elemDb.GetElementByKey(currentElement.Logic);
            if (tmplElem.ElementKey.Equals(""))
            {
                return "";
            }
            var parentKey = Cache.RoomCache.Room.ElementKey;
            if (tmplElem.ElementType.Equals("room"))
            {
                parentKey = "";
            }
            tmplElem.ParentKey = parentKey;
            var newElemTags = Tools.OutputProcessor.ProcessOutputText(currentElement.Output, currentElement);
            var doSpawn = Tools.RepeatProcessor.ProcessSpawnRepeat(currentElement);

            if (doSpawn)
            {
                var key = elemDb.SpawnTemplateElement(currentElement, currentElement.Logic, newElemTags);
                return key;
            }

            return "";
        }
    }
}
