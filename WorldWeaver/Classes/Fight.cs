using System;
namespace WorldWeaver.Classes
{
    public class Fight
    {
        public bool PlayersTurn { get; set; } = true;

        public bool InitialRound { get; set; } = true;

        public bool RoundHandled { get; set; } = false;
        public List<Classes.Element> Enemies { get; set; } = new List<Classes.Element>();
        public Classes.Element Target { get; set; } = new Element();
        public bool PlayerHasAttacked { get; set; }
        public bool PlayerFleeing { get; set; } = false;

        public bool AllEnemiesDead { get; set; } = false;
    }
}
