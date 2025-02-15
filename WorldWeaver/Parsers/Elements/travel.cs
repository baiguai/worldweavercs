using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers.Elements
{
    public class Travel
    {
        public void ParseTravel()
        {
            var elemParser = new Elements.Element();
            var elemLogic = new DataManagement.GameLogic.Element();
            var allTravels = elemLogic.GetElementsByType("travel");

            if (Cache.FightCache.Fight != null)
            {
                return;
            }

            foreach (var trv in allTravels)
            {
                var trvParent = Tools.Elements.GetSelf(trv);
                var msgs = trv.Children.Where(c => c.ElementType.Equals("message", StringComparison.OrdinalIgnoreCase)).ToList();
                if (trv.Active.ToLower().Equals("true"))
                {
                    if (!trv.Logic.Equals(""))
                    {
                        string[]? arr = null;

                        if (trv.Logic.StartsWith("[follow|"))
                        {
                            arr = trv.Logic.Replace("[follow|", "").Replace("]", "").Split('|');

                            if (arr.Length == 1)
                            {
                                var target = arr[0].Trim();

                                if (target.Equals(Cache.RoomCache.Room.ElementKey))
                                {
                                    elemLogic.SetElementParentKey(trvParent.ElementKey, Cache.RoomCache.Room.ElementKey);
                                    if (MainClass.output.PlayerMoved)
                                    {
                                        ParseTravelMessages(trv, msgs);
                                    }
                                    continue;
                                }
                                if (target.Equals(Cache.PlayerCache.Player.ElementKey))
                                {
                                    elemLogic.SetElementParentKey(trvParent.ElementKey, Cache.PlayerCache.Player.ParentKey);
                                    if (MainClass.output.PlayerMoved)
                                    {
                                        ParseTravelMessages(trv, msgs);
                                    }
                                    continue;
                                }

                                var targetElem = elemLogic.GetElementByKey(target);
                                elemLogic.SetElementParentKey(trvParent.ElementKey, targetElem.ParentKey);
                                continue;
                            }
                        }

                        if (trv.Logic.StartsWith("[path|"))
                        {
                            arr = trv.Logic.Replace("[path|", "").Replace("]", "").Split('|');

                            if (arr.Length >= 1)
                            {
                                var index = trv.RepeatIndex;
                                if (index < 0)
                                {
                                    index = 0;
                                }
                                if (index >= arr.Length)
                                {
                                    index = 0;
                                }
                                elemLogic.SetElementParentKey(trvParent.ElementKey, arr[index]);
                                index++;
                                var dbElem = new DataManagement.GameLogic.Element();
                                dbElem.SetElementField(
                                    trv.ElementKey,
                                    "RepeatIndex",
                                    index.ToString(),
                                    false
                                );
                            }
                        }
                    }
                }
            }

            CacheManager.RefreshCache();
        }

        public void ParseTravelMessages(Classes.Element travelElem, List<Classes.Element>? messages)
        {
            if (MainClass.output.FailedLogic)
            {
                return;
            }
            var msgParser = new Parsers.Elements.Message();
            var lgcParser = new Parsers.Elements.Logic();

            foreach (var msg in messages)
            {
                lgcParser.ParseLogic(msg);
                if (!MainClass.output.FailedLogic)
                {
                    MainClass.output.OutputText += Environment.NewLine + Tools.OutputProcessor.ProcessOutputText(msg.Output, msg);
                }
                else
                {
                    // Reset the failed logic, so the existing output is shown.
                    MainClass.output.FailedLogic = false;
                }
            }
        }
    }
}
