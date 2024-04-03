using System;
using System.Collections.Generic;
using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class ConditionalVariable
    {
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";
        public Classes.Element Element { get; set; } = new Classes.Element();
        public string Condition { get; set; } = "";
        public string SubCondition { get; set; } = "";
        public string SubValue { get; set; } = "";
        public int SubValueInt { get; set; } = 0;
    }
}
