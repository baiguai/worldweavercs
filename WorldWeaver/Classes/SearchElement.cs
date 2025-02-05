﻿using System;
using System.Collections.Generic;
using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class SearchElement
    {
        public string ElementKey { get; set; } = "";
        public string Syntax { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Active { get; set; } = true;
    }
}
