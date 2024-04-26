using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class CacheManager
    {
        public static void RefreshCache()
        {
            var elemLogic = new DataManagement.GameLogic.Element();
            var logic = new DataManagement.GameLogic.Element();

            Cache.GameCache.Game = null;
            Cache.PlayerCache.Player = new Element();
            Cache.RoomCache.Room = new Element();
            Cache.GlobalCache.Global = new List<Element>();

            var gameElem = elemLogic.GetElementsByType(MainClass.gameDb, "game")[0];
            var player = logic.GetElementByKey(MainClass.gameDb, "player");
            var locElem = logic.GetElementByKey(MainClass.gameDb, player.ParentKey);
            var globalElems = logic.GetElementsByType(MainClass.gameDb, "global");
            var eventElems = logic.GetElementsByType(MainClass.gameDb, "event");

            Cache.GameCache.Game = gameElem;
            Cache.PlayerCache.Player = player;
            Cache.RoomCache.Room = locElem;
            Cache.GlobalCache.Global = globalElems;
            Cache.EventCache.Event = eventElems;
        }

        internal static void ClearCache()
        {
            Cache.GameCache.Game = null;
            Cache.PlayerCache.Player = new Classes.Element();
            Cache.RoomCache.Room = new Classes.Element();
            Cache.GlobalCache.Global.Clear();
            Cache.EventCache.Event.Clear();
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

            if (elem == null)
            {
                foreach (var evnt in Cache.EventCache.Event)
                {
                    if (evnt.ElementKey.Equals(key) && !evnt.ElementKey.Equals(""))
                    {
                        elem = evnt;
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

                case "event":
                    elems.AddRange(Cache.EventCache.Event);
                    break;
            }

            return elems;
        }
    }
}