using System;
namespace WorldWeaver.Parsers.Elements
{
    public class Helpers
    {
        public static bool HasEnterMessage(Classes.Element element)
        {
            var enterMsgOutput = false;

            foreach (var child in element.Children)
            {
                if (child.ElementType.Equals("enter_message"))
                {
                    return true;
                }
            }

            return enterMsgOutput;
        }

        public static Classes.Element GetChildByType(Classes.Element currentElement, string type)
        {
            Classes.Element childOutput = null;

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals(type))
                {
                    return child;
                }
            }

            return childOutput;
        }
    }
}
