using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Classes;

namespace WorldWeaver.Tools
{
    public class Template
    {
        public static string GetTemplateParent(Classes.Element currentElement, string templateParentValue)
        {
            var updatedValue = templateParentValue;

            switch (templateParentValue)
            {
                case "[room]":
                    updatedValue = Cache.RoomCache.Room.ElementKey;
                    break;

                case "[player]":
                    updatedValue = Cache.PlayerCache.Player.ElementKey;
                    break;

                case "[enemy]":
                    if (Cache.FightCache.Fight != null)
                    {
                        updatedValue = Cache.FightCache.Fight.Enemy.ElementKey;
                    }
                    break;

                case "[self]":
                    updatedValue = Tools.Elements.GetSelf(currentElement).ElementKey;
                    break;

                case "[current]":
                    updatedValue = currentElement.ElementKey;
                    break;

                case "[parent]":
                    updatedValue = currentElement.ParentKey;
                    break;
            }

            return updatedValue;
        }

        internal static string GetTemplateName(Element currentElement, string name)
        {
            var newName = name;

            if (name.Contains("|"))
            {
                var arr = name.Split('|');
                newName = arr[Tools.ValueTools.Randomize(0, arr.Length-1)];
            }

            return newName;
        }
    }
}