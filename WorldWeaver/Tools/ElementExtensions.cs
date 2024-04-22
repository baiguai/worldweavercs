using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldWeaver.Tools
{
    public static class ElementExtensions
    {
        public static Classes.Element? AttributeByTag(this Classes.Element currentElement, string tag)
        {
            Classes.Element? attrib = null;

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals("attribute") && child.Tags.TagsContain(tag))
                {
                    attrib = child;
                    break;
                }
            }

            return attrib;
        }

        public static List<Classes.Element> AttributesByTag(this Classes.Element currentElement, string tag)
        {
            var attribs = new List<Classes.Element>();

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals("attribute") && child.Tags.TagsContain(tag))
                {
                    attribs.Add(child);
                }
            }

            return attribs;
        }

        public static Classes.Element? ChildByKey(this Classes.Element currentElement, string childKey)
        {
            Classes.Element? selChild = null;

            foreach (var child in currentElement.Children)
            {
                if (child.ElementKey.Equals(childKey))
                {
                    selChild = child;
                    break;
                }
            }

            return selChild;
        }

        public static Classes.Element? ChildByTag(this Classes.Element currentElement, string tag)
        {
            Classes.Element? selChild = null;

            foreach (var child in currentElement.Children)
            {
                if (child.Tags.TagsContain(tag))
                {
                    selChild = child;
                    break;
                }
            }

            return selChild;
        }

        public static Classes.Element? ChildByType(this Classes.Element currentElement, string type)
        {
            Classes.Element? selChild = null;

            foreach (var child in currentElement.Children)
            {
                if (child.ElementType.Equals(type))
                {
                    selChild = child;
                    break;
                }
            }

            return selChild;
        }
    }
}
