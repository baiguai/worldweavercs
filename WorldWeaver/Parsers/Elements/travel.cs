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

            foreach (var trv in allTravels)
            {
                var trvParent = Tools.Elements.GetSelf(trv);
                var msgs = trv.Children.Where(c => c.ElementType.Equals("message", StringComparison.OrdinalIgnoreCase)).ToList();
                if (trv.Active.ToLower().Equals("true"))
                {
                    if (!trv.Logic.Equals(""))
                    {
                        var arr = trv.Logic.Replace("[", "").Replace("]", "").Split('|');

                        if (trv.Logic.StartsWith("[follow|"))
                        {
                            if (arr.Length == 2)
                            {
                                var target = arr[1].Trim();

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

                        if (trv.Logic.StartsWith("[path|]"))
                        {
                            if (arr.Length >= 2)
                            {
                                var parentKey = "";
                                for (var ix = 1; ix < arr.Length; ix++)
                                {
                                    if (trvParent.ElementKey.Equals(arr[ix]))
                                    {
                                        if (ix == arr.Length)
                                        {
                                            parentKey = arr[1];
                                            elemLogic.SetElementParentKey(trvParent.ElementKey, parentKey);
                                            break;
                                        }
                                        else
                                        {
                                            parentKey = arr[ix + 1];
                                            elemLogic.SetElementParentKey(trvParent.ElementKey, parentKey);
                                            break;
                                        }
                                    }
                                }
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
