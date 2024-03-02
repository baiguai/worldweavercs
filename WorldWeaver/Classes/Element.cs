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
        public string location { get; set; }
        public string syntax { get; set; }
        public string logic { get; set; }
        public string repeat { get; set; } = "repeat";
        public int repeat_index { get; set; } = 0;
        public string output { get; set; }
        public string tags { get; set; }
        public string active { get; set; } = "1";
        public List<Element> children { get; set; }
        public int sort { get; set; } = 1;
        public string create_date { get; set; } = DateTime.Now.FormatDate();
        public string update_date { get; set; } = DateTime.Now.FormatDate();
    }
}
