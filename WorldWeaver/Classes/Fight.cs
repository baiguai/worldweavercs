﻿using System;
namespace WorldWeaver.Classes
{
    public class Fight
    {
        public bool PlayersTurn { get; set; } = true;
        public Classes.Element Enemy { get; set; } = new Classes.Element();
    }
}