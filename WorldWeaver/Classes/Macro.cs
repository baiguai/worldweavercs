﻿using System;
namespace WorldWeaver.Classes
{
    public class Macro
    {
        public string MacroName { get; set; } = "";
        public bool IsRecording { get; set; } = false;
        public List<string> MacroSteps { get; set; } = new List<string>();
    }
}