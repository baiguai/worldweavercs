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
        public string SpawnByTemplate(Classes.Element currentElement, string templateKey, string newElemKey)
        {
            DataManagement.GameLogic.Element elemDb = new DataManagement.GameLogic.Element();

            var tmplElem = elemDb.GetElementByKey(templateKey);
            var key = newElemKey;
            if (key.Equals(""))
            {
                key = Guid.NewGuid().ToString();
            }

            elemDb.SpawnTemplateElement(currentElement, templateKey, key);

            return key;
        }
    }
}
