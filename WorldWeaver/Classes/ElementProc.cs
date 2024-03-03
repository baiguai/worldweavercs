using System;
using System.Collections.Generic;

namespace WorldWeaver.Classes
{
    public class ElementProc
    {
        public List<string> CurrentElementTypes { get; set; } = new List<string>();
        public List<string> ChildProcElements { get; set; } = new List<string>();
        public bool AllowRepeatOptions { get; set; }
    }
}
