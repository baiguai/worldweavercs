namespace WorldWeaver.Tools
{
    public class Elements
    {
        public static List<Classes.Element> GetElementsByType(Classes.Element currentElement, string type)
        {
            var output = new List<Classes.Element>();

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    output.Add(child);
                }

                output.AddRange(GetElementsByType(child, type));
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

                default:
                    return curElement.Output;
            }
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

                case "[enemy]":
                    if (Cache.FightCache.Fight == null)
                    {
                        return new Classes.Element();
                    }
                    return Cache.FightCache.Fight.Enemy;

                default:
                    return Cache.RoomCache.Room;
            }
        }
    }
}