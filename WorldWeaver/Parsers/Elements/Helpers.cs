using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Helpers
    {
        public static bool HasEnterMessage(Classes.Element element)
        {
            var output = false;

            foreach (var child in element.Children)
            { 
                if (child.ElementType.Equals("enter_message"))
                {
                    return true;
                }
            }

            return output;
        }

        public static Classes.Element GetChildByType(Classes.Element currentElement, string type)
        {
            Classes.Element output = null;

            foreach (var child in currentElement.Children)
            { 
                if (child.ElementType.Equals(type))
                {
                    return child;
                }
            }

            return output;
        }
    }
}
