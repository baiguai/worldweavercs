﻿using System;
using WorldWeaver.Classes;

namespace WorldWeaver.Parsers.Elements
{
    public class Action
    {
        internal Classes.Output ParseAction(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var elem = new Parsers.Elements.Element();
            var procItems = Tools.ProcFunctions.GetProcessStepsByType(currentElement.ElementType);

            output = ParseMessageActions(output, gameDb, currentElement, userInput);

            foreach (var child in currentElement.Children)
            {
                foreach (var proc in procItems)
                {
                    output = elem.ParseElement(output, gameDb, currentElement, userInput, proc);
                }
                if (output.MatchMade)
                {
                    break;
                }
            }

            return output;
        }

        internal Classes.Output ParseMessageActions(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            var tags = currentElement.Tags.Split('|');

            if (tags.Contains("list", StringComparer.OrdinalIgnoreCase))
            {
                output = ParseTags_List(output, gameDb, currentElement, userInput);
            }

            if (tags.Contains("type", StringComparer.OrdinalIgnoreCase))
            {
                output = ParseTags_Type(output, gameDb, currentElement, userInput);
            }

            return output;
        }

        private Output ParseTags_Type(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            if (Cache.RoomCache.Room == null)
            {
                return output;
            }
            var type = currentElement.Logic.Trim();
            var targets = Tools.Elements.GetElementsByType(Cache.RoomCache.Room, type);

            var elemParser = new Parsers.Elements.Element();
            
            foreach (var elem in targets)
            {
                var procItems = Tools.ProcFunctions.GetProcessStepsByType(elem.ElementType);
                foreach (var proc in procItems)
                {
                    output = elemParser.ParseElement(output, gameDb, elem, userInput, proc);
                }
            }

            return output;
        }

        private Output ParseTags_List(Output output, string gameDb, Classes.Element currentElement, string userInput)
        {
            throw new NotImplementedException();
        }
    }
}