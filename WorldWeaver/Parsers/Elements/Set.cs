using System;
using System.Diagnostics.Tracing;
using WorldWeaver.Tools;
namespace WorldWeaver.Parsers.Elements
{
    public class Set
    {
        public void ParseSet(Classes.Element parentElement, Classes.Element currentElement)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var logParse = new Parsers.Elements.Logic();
            var logic = currentElement.Logic;
            var targetKey = "";
            var targetField = "";
            var newValue = "";
            var usingInput = false;

            MainClass.output.MatchMade = false;

            if (logic.Contains("[input]"))
            {
                var foundElems = new List<string>();

                if (currentElement.Tags.TagsContain("child"))
                {
                    var tag = currentElement.Tags.RemoveTag("child");
                    foundElems = elemDb.GetChildElementKeysBySyntax(Tools.Elements.GetSelf(currentElement), MainClass.userInput.Replace(tag, ""), true);
                }
                else if (currentElement.Tags.TagsContain("[player]"))
                {
                    var tag = currentElement.Tags.RemoveTag("[player]");
                    foundElems = elemDb.GetChildElementKeysBySyntax(Cache.PlayerCache.Player, MainClass.userInput.Replace(tag, ""), true);
                }
                else
                {
                    foundElems = elemDb.GetElementKeysBySyntax(MainClass.userInput.Replace(currentElement.Tags.Trim(), ""));
                }


                if (foundElems.Count < 1)
                {
                    MainClass.output.OutputText += "I'm not sure what object you are referencing.";
                    MainClass.output.MatchMade = false;
                    return;
                }
                if (foundElems.Count > 1)
                {
                    MainClass.output.OutputText += "There are multiple items that match the name you specified.";
                    MainClass.output.MatchMade = false;
                    return;
                }
                logic = logic.Replace("[input]", foundElems.First());
            }
            logic = logParse.ParseSetLogic(logic, parentElement.Tags);

            var arr = logic.Split('(');
            if (arr.Length == 2)
            {
                targetKey = arr[0].Trim();
            }

            arr = arr[1].Split(')');
            if (arr.Length == 2)
            {
                targetField = arr[0].Trim();
                newValue = arr[1].Trim();

                if (targetKey.Equals("gamestate"))
                {
                    SetGameState(targetField, newValue);
                    MainClass.output.MatchMade = true;
                    return;
                }

                var targetElement = elemDb.GetElementByKey(targetKey);

                if (targetElement.ElementKey.Equals(""))
                {
                    MainClass.output.OutputText += "An error occurred while setting a value.";
                    MainClass.output.MatchMade = false;
                    return;
                }

                if (!currentElement.Tags.Equals(""))
                {
                    // This allows the game to remove additional text if needed
                    newValue = newValue.Replace(currentElement.Tags, "").Trim();
                }

                if (targetField.Equals("tags"))
                {
                    newValue = NewTagsValue(targetElement, newValue);
                }

                elemDb.SetElementField(targetElement.ElementKey, targetField, newValue);

                MainClass.output.MatchMade = true;
            }
            else
            {
                MainClass.output.OutputText += "An error has occured while setting a value.";
                MainClass.output.MatchMade = false;
                return;
            }

            return;
        }

        private void SetGameState(string targetField, string newValue)
        {
            if (targetField != "MissionDays")
            {
                return;
            }
            var gameData = new DataManagement.GameLogic.Game();
            gameData.UpdateGameState(targetField, newValue);
        }

        private List<Classes.Element> GetTargetElements(string gameDb, Classes.Element currentElement, string targetString)
        {
            var targetElements = new List<Classes.Element>();
            var elemDb = new DataManagement.GameLogic.Element();
            var key = "";
            var type = "";
            var tag = "";

            if (targetString.Equals("[self]"))
            {
                targetElements.Add(Tools.Elements.GetSelf(currentElement));
                return targetElements;
            }

            var arr = targetString.Split("((");
            if (arr.Length == 2)
            {
                key = arr[0].Trim();
                tag = arr[1].Trim().Replace("))", "");

                var tmpElem = new Classes.Element();
                if (key.Equals("[self]"))
                {
                    tmpElem = Tools.Elements.GetSelf(currentElement);
                }
                else
                {
                    tmpElem = elemDb.GetElementByKey(key);
                }

                if (tmpElem.Tags.Contains(tag))
                {
                    targetElements.Add(tmpElem);
                }

                targetElements.AddRange(tmpElem.Children.Where(c => c.Tags.Contains(tag)));
                return targetElements;
            }

            arr = targetString.Split("(");
            if (arr.Length == 2)
            {
                key = arr[0].Trim();
                type = arr[1].Trim().Replace(")", "");

                var tmpElem = new Classes.Element();
                if (key.Equals("[self]"))
                {
                    tmpElem = Tools.Elements.GetSelf(currentElement);
                }
                else
                {
                    tmpElem = elemDb.GetElementByKey(key);
                }

                if (tmpElem.ElementType.Equals(type))
                {
                    targetElements.Add(tmpElem);
                }

                targetElements.AddRange(tmpElem.Children.Where(c => c.ElementType.Equals(type)));
                return targetElements;
            }

            targetElements.Add(elemDb.GetElementByKey(targetString));

            return targetElements;
        }

        private string NewTagsValue(Classes.Element targetElement, string newValue)
        {
            var output = "";

            if (newValue.Length > 0)
            {
                switch (newValue.Substring(0, 1))
                {
                    case "+":
                        newValue = newValue.Replace("+", "");
                        output = targetElement.Tags.AddTag(newValue);
                        break;

                    case "-":
                        newValue = newValue.Replace("-", "");
                        output = targetElement.Tags.RemoveTag(newValue);
                        break;

                    case "*":
                        newValue = newValue.Replace("*", "");
                        var nvArr = newValue.Split('>');
                        if (nvArr.Length == 2)
                        {
                            output = targetElement.Tags.ReplaceTag(nvArr[0].Trim(), nvArr[1].Trim());
                        }
                        break;
                }
            }

            return output;
        }
    }
}
