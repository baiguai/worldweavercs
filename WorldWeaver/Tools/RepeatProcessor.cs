using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class RepeatProcessor
    {
        internal static bool ProcessSpawnRepeat(Element currentElement)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var doSpawn = false;

            if (currentElement.Repeat.Equals("*"))
            {
                return true;
            }

            if (int.TryParse(currentElement.Repeat, out _))
            {
                if (Convert.ToInt32(currentElement.Repeat) > 0)
                {
                    var curRpt = Convert.ToInt32(currentElement.Repeat);
                    elemDb.SetElementField(currentElement.ElementKey, "repeat", (curRpt-1).ToString(), true);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            var lgcElem = elemDb.GetElementByKey(currentElement.Repeat);
            if (!lgcElem.ElementKey.Equals("") && lgcElem.ElementType.Equals("logic"))
            {
                var lgcParser = new Parsers.Elements.Logic();
                lgcParser.ParseLogic(lgcElem);
                if (!MainClass.output.FailedLogic)
                {
                    return true;
                }
            }

            return doSpawn;
        }
    }
}
