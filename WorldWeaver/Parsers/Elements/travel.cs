﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Travel
    {
        public void ParseTravel(string gameDb)
        {
            var elemParser = new Elements.Element();
            var elemLogic = new DataManagement.GameLogic.Element();
            var allTravels = elemLogic.GetElementsByType(gameDb, "travel");

            foreach (var trv in allTravels)
            {
                var trvParent = Tools.Elements.GetSelf(gameDb, trv);
                if (trv.Active.ToLower().Equals("true"))
                {
                    if (!trv.Logic.Equals(""))
                    {
                        if (trv.Logic.StartsWith("[follow|"))
                        {
                            var arr = trv.Logic.Replace("[", "").Replace("]", "").Split('|');
                            if (arr.Length == 2)
                            {
                                var target = arr[1].Trim();

                                if (target.Equals(Cache.RoomCache.Room.ElementKey))
                                {
                                    elemLogic.SetElementParentKey(gameDb, trvParent.ElementKey, Cache.RoomCache.Room.ElementKey);
                                    continue;
                                }
                                if (target.Equals(Cache.PlayerCache.Player.ElementKey))
                                {
                                    elemLogic.SetElementParentKey(gameDb, trvParent.ElementKey, Cache.PlayerCache.Player.ParentKey);
                                    continue;
                                }
                                
                                var targetElem = elemLogic.GetElementByKey(gameDb, target);
                                elemLogic.SetElementParentKey(gameDb, trvParent.ElementKey, targetElem.ParentKey);
                                continue;
                            }
                        }
                    }
                }
            }

            CacheManager.RefreshCache(gameDb);
        }
    }
}