using System;
using System.Collections.Generic;
using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class Element
    {
        public string element_type { get; set; }
        public string element_key { get; set; }
        public string name { get; set; }
        public string parent_key { get; set; }
        public string syntax { get; set; }
        public string logic { get; set; }
        public string output { get; set; }
        public string tags { get; set; }
        public string active { get; set; }
        public List<Element> children { get; set; }
        public int sort { get; set; } = 1;
        public string create_date { get; set; } = DateTime.Now.FormatDate();
        public string update_date { get; set; } = DateTime.Now.FormatDate();
    }
}
