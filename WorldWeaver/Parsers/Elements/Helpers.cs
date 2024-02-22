using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Helpers
    {
        public static bool HasEnterMessage(Classes.Element element)
        {
            var output = false;

            foreach (var child in element.children)
            { 
                if (child.element_type.Equals("enter_message"))
                {
                    return true;
                }
            }

            return output;
        }

        public static Classes.Element GetChildByType(Classes.Element currentElement, string type)
        {
            Classes.Element output = null;

            foreach (var child in currentElement.children)
            { 
                if (child.element_type.Equals(type))
                {
                    return child;
                }
            }

            return output;
        }
    }
}
