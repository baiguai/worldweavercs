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

        public static Classes.Element GetSelf(string gameDb, Classes.Element currentElement)
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
                var elem = dbElem.GetElementByKey(gameDb, currentElement.ParentKey);
                return GetSelf(gameDb, elem);
            }
        }

        public static int GetLife(Classes.Element currentElement)
        {
            var life = 0;

            try
            {
                life = Convert.ToInt32(currentElement.Children.Select(c => c.Tags.TagsContain("life")).First());
            }
            catch (Exception)
            {
                life = 0;
            }

            return life;
        }
    }
}