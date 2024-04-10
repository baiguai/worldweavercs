using System;
using System.Collections.Generic;
using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class Element
    {
        public string ElementType { get; set; } = "";
        public string ElementKey { get; set; } = "";
        public string Name { get; set; } = "";
        public string ParentKey { get; set; } = "";
        public string Syntax { get; set; } = "";
        public string Logic { get; set; } = "";
        public string Repeat { get; set; } = "repeat";
        public int RepeatIndex { get; set; } = 0;
        public string Output { get; set; } = "";
        public string Tags { get; set; } = "";
        public string Active { get; set; } = "true";
        public List<Element> Children { get; set; } = new List<Element>();
        public int Sort { get; set; } = 1;
        public string CreateDate { get; set; } = DateTime.Now.FormatDate();
        public string UpdateDate { get; set; } = DateTime.Now.FormatDate();
    }
}
