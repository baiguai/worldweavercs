using System;
namespace WorldWeaver.Classes
{
    public class Fight
    {
        public bool PlayersTurn { get; set; } = true;
        public List<Classes.Element> Enemies { get; set; } = new List<Classes.Element>();
        public Classes.Element Target { get; set; } = new Element();
        public bool PlayerHasAttacked { get; set; }
        public bool PlayerFleeing { get; set; } = false;
    }
}
