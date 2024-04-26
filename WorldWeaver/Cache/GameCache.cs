using System;
namespace WorldWeaver.Cache
{
    public class GameCache
    {
        public static Classes.Element? Game { get; set; }
        public static bool GameInitialized { get; set; } = false;
    }
}
