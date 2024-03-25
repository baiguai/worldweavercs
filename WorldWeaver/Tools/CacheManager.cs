using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class CacheManager
    {
        public static void RefreshCache(string gameDb)
        {
            var elemLogic = new DataManagement.GameLogic.Element();
            var logic = new DataManagement.GameLogic.Element();

            Cache.GameCache.Game = null;
            Cache.PlayerCache.Player = new Element();
            Cache.RoomCache.Room = new Element();
            Cache.GlobalCache.Global = new List<Element>();

            var gameElem = elemLogic.GetElementsByType(gameDb, "game")[0];
            var player = logic.GetElementByKey(gameDb, "player");
            var locElem = logic.GetElementByKey(gameDb, player.ParentKey);
            var globalElems = logic.GetElementsByType(gameDb, "global");

            Cache.GameCache.Game = gameElem;
            Cache.PlayerCache.Player = player;
            Cache.RoomCache.Room = locElem;
            Cache.GlobalCache.Global = globalElems;
        }

        internal static void ClearCache()
        {
            Cache.GameCache.Game = null;
            Cache.PlayerCache.Player = new Classes.Element();
            Cache.RoomCache.Room = new Classes.Element();
            Cache.GlobalCache.Global.Clear();
        }

        internal static Element? GetCachedElement(string key)
        {
            Classes.Element elem = null;

            switch (key)
            {
                case "player":
                    if (Cache.PlayerCache.Player != null && !Cache.PlayerCache.Player.ElementKey.Equals(""))
                    {
                        elem = Cache.PlayerCache.Player;
                    }
                    break;

                case "game":
                    if (Cache.GameCache.Game != null && !Cache.GameCache.Game.ElementKey.Equals(""))
                    {
                        elem = Cache.GameCache.Game;
                    }
                    break;
            }

            if (elem == null)
            {
                foreach (var glob in Cache.GlobalCache.Global)
                {
                    if (glob.ElementKey.Equals(key) && !glob.ElementKey.Equals(""))
                    {
                        elem = glob;
                        break;
                    }
                }
            }

            return elem;
        }

        internal static List<Element> GetCachedElementByType(string type)
        {
            var elems = new List<Classes.Element>();

            switch (type)
            {
                case "game":
                    if (Cache.GameCache.Game != null && !Cache.GameCache.Game.ElementKey.Equals(""))
                    {
                        elems.Add(Cache.GameCache.Game);
                    }
                    break;

                case "player":
                    if (Cache.PlayerCache.Player != null && !Cache.PlayerCache.Player.ElementKey.Equals(""))
                    {
                        elems.Add(Cache.PlayerCache.Player);
                    }
                    break;

                case "global":
                    elems.AddRange(Cache.GlobalCache.Global);
                    break;
            }

            return elems;
        }
    }
}