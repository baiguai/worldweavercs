
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class Elements
    {
        public static List<Classes.Element> GetElementsByType(Classes.Element currentElement, string type, bool includeChildren)
        {
            var output = new List<Classes.Element>();

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    output.Add(child);
                }

                if (includeChildren)
                {
                    output.AddRange(GetElementsByType(child, type, includeChildren));
                }
            }

            return output;
        }

        public static Classes.Element GetSelf(Classes.Element currentElement)
        {
            var self = new Classes.Element();
            var types = Tools.AppSettingFunctions.GetRootArray("Config/SelfTypes.json");
            var dbElem = new DataManagement.GameLogic.Element();

            if (types.Contains(currentElement.ElementType))
            {
                return currentElement;
            }
            else
            {
                var elem = dbElem.GetElementByKey(currentElement.ParentKey);
                return GetSelf(elem);
            }
        }

        public static int GetLife(Classes.Element currentElement)
        {
            var life = 0;

            try
            {
                var lifeElem = currentElement.ChildByTag("life");
                life = Convert.ToInt32(lifeElem.Output);
            }
            catch (Exception)
            {
                life = 0;
            }

            return life;
        }

        public static string GetElementProperty(Classes.Element curElement, string elementProperty)
        {
            var arr = elementProperty.Split('|');
            var action = "";
            if (arr.Length == 2)
            {
                action = arr[0].Trim();
                elementProperty = arr[1].Trim();
            }

            if (action.Equals("link"))
            {
                var elemDb = new DataManagement.GameLogic.Element();
                var elem = elemDb.GetElementByKey(curElement.Output);
                curElement = elem;
            }

            elementProperty = Tools.Elements.FixPropertyName(elementProperty);

            switch (elementProperty.ToLower())
            {
                case "elementkey":
                    return curElement.ElementKey;

                case "elementtype":
                    return curElement.ElementType;

                case "parentkey":
                    return curElement.ParentKey;

                case "syntax":
                    return curElement.Syntax;

                case "logic":
                    return curElement.Logic;

                case "tags":
                    return curElement.Tags;

                case "active":
                    return curElement.Active;

                case "name":
                    return curElement.Name;

                default:
                    return Tools.OutputProcessor.ProcessOutputText(curElement.Output.RandomValue(curElement), curElement);
            }
        }

        public static string FixPropertyName(string elementProperty)
        {
            if (elementProperty.Equals("parent"))
            {
                elementProperty = "parentkey";
            }
            if (elementProperty.Equals("type"))
            {
                elementProperty = "elementtype";
            }
            if (elementProperty.Equals("key"))
            {
                elementProperty = "elementkey";
            }

            switch (elementProperty.ToLower())
            {
                case "elementkey":
                    return "ElementKey";

                case "elementtype":
                    return "ElementType";

                case "parentkey":
                    return "ParentKey";

                case "syntax":
                    return "Syntax";

                case "logic":
                    return "Logic";

                case "tags":
                    return "Tags";

                case "active":
                    return "Active";

                case "name":
                    return "Name";

                case "repeat":
                    return "Repeat";

                case "output":
                    return "Output";
            }

            return elementProperty;
        }

        public static Classes.Element GetRelativeElement(Classes.Element currentElement, string relCode)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            switch (relCode.ToLower())
            {
                case "[self]":
                    return Tools.Elements.GetSelf(currentElement);

                case "[room]":
                    return Cache.RoomCache.Room;

                case "[parent]":
                    return elemDb.GetElementByKey(Tools.Elements.GetSelf(currentElement).ParentKey);

                case "[player]":
                    return Cache.PlayerCache.Player;

                case "[enemy]":
                    if (Cache.FightCache.Fight == null)
                    {
                        return new Classes.Element();
                    }
                    return Cache.FightCache.Fight.Target;

                default:
                    return elemDb.GetElementByKey(relCode.ToLower().Replace("[", "").Replace("]", ""));
            }
        }

        internal static Classes.Element GetRelativeElementKey(Element currentElement, string relCode)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            switch (relCode.ToLower())
            {
                case "[self]":
                    return Tools.Elements.GetSelf(currentElement);

                case "[room]":
                    return Cache.RoomCache.Room;

                case "[player]":
                    return Cache.PlayerCache.Player;

                case "[enemy]":
                    if (Cache.FightCache.Fight == null)
                    {
                        return null;
                    }
                    return Cache.FightCache.Fight.Target;

                default:
                    return null;
            }
        }

        internal static ConsoleColor GetColor(string colorString)
        {
            switch (colorString.ToLower())
            {
                case "blue":
                    return ConsoleColor.Blue;

                case "cyan":
                    return ConsoleColor.Cyan;

                case "darkblue":
                    return ConsoleColor.DarkBlue;

                case "darkcyan":
                    return ConsoleColor.DarkCyan;

                case "darkgray":
                    return ConsoleColor.DarkGray;

                case "darkgreen":
                    return ConsoleColor.DarkGreen;

                case "darkmagenta":
                    return ConsoleColor.DarkMagenta;

                case "darkred":
                    return ConsoleColor.DarkRed;

                case "darkyellow":
                    return ConsoleColor.DarkYellow;

                case "gray":
                    return ConsoleColor.Gray;

                case "green":
                    return ConsoleColor.Green;

                case "magenta":
                    return ConsoleColor.Magenta;

                case "red":
                    return ConsoleColor.Red;

                case "yellow":
                    return ConsoleColor.Yellow;

                default:
                    return ConsoleColor.White;
            }
        }

        internal static string FixTagsUpdateValue(string element_key, string new_value)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var elem = elemDb.GetElementByKey(element_key);

            if (new_value.Equals(""))
            {
                return new_value;
            }

            if (new_value.StartsWith('+'))
            {
                new_value = new_value.Substring(1);
                return elem.Tags.AddTag(new_value);
            }

            if (new_value.StartsWith('-'))
            {
                new_value = new_value.Substring(1);
                return elem.Tags.RemoveTag(new_value);
            }

            return new_value;
        }

        internal static bool IsCustomType(string elementType)
        {
            if (!Tools.ProcFunctions.GetElementTypes().Contains(elementType))
            {
                return true;
            }

            return false;
        }

        internal static List<string> FailedLogicResetTypes()
        {
            return Tools.AppSettingFunctions.GetRootArray("Config/ResetFailedTypes.json");
        }

        public static Classes.Element GetWeapon(Classes.Element parentElem)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            if (parentElem.Equals(Cache.PlayerCache.Player))
            {
                return elemDb.GetElementByKey(Cache.PlayerCache.Player.AttributeByTag("armed").Output);
            }
            if (Cache.FightCache.Fight != null)
            {
                return parentElem.AttributeByTag("armed");
            }

            return null;
        }
    }
}