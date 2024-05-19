using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorldWeaver.Cache;
using WorldWeaver.Tools;

namespace WorldWeaver.Parsers
{
    public class DevToolsParser
    {
        public void ParseInput()
        {
            MainClass.output.MatchMade = false;
            MainClass.output.OutputText = "";

            var method = Tools.CommandFunctions.GetCommandMethod(MainClass.userInput, "DevToolsParser");

            if (!method.Equals(""))
            {
                if (!MainClass.output.MatchMade && method.Equals("DoAdminLogin"))
                {
                    DoAdminLogin(MainClass.userInput.GetInputParamSingle());
                }
                if (!MainClass.output.MatchMade && method.Equals("DoList"))
                {
                    DoList();
                }
                if (!MainClass.output.MatchMade && method.Equals("DoMap"))
                {
                    DoMap();
                }
            }
        }

        private void DoAdminLogin(string userInput)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            var pwdElem = elemDb.GetElementChildByTag(GameCache.Game.ElementKey, "adminpass");
            if (pwdElem.ElementKey.Equals(""))
            {
                return;
            }

            if (userInput.Equals(pwdElem.Output))
            {
                MainClass.adminEnabled = true;
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = "Admin mode enabled.";
            }
        }

        private void DoMap()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var elemDb = new DataManagement.GameLogic.Element();
            var rooms = elemDb.GetElementsByType("room");
            var map = "";
            var nl = Environment.NewLine;
            var space = $"{Environment.NewLine}{Environment.NewLine}";
            var indent = "   ";

            foreach (var room in rooms)
            {
                if (!map.Equals(""))
                {
                    map += space;
                }

                map += $"Room: {room.Name} ({room.ElementKey}){nl}";
                var doors = elemDb.GetDoors(room.ElementKey);

                foreach (var door in doors)
                {
                    map += $"{indent}Door: {door.ElementKey}{nl}";
                    map += $"{indent}{indent}Syntax: {Tools.RegexTools.RegexScrub(door.Syntax)}{nl}";
                    map += $"{indent}{indent}Target: {door.Logic}{nl}";
                    map += nl;
                }

                var npcs = elemDb.GetElementChildrenByType(room.ElementKey, "npc");

                foreach (var npc in npcs)
                {
                    map += $"{indent}NPC: {npc.Name} ({npc.ElementKey}){nl}";
                    map += nl;
                }

                var objects = elemDb.GetElementChildrenByType(room.ElementKey, "object");

                foreach (var obj in objects)
                {
                    map += $"{indent}Object: {obj.Name} ({obj.ElementKey}){nl}";
                    map += nl;
                }
            }

            if (!map.Equals(""))
            {
                MainClass.output.OutputText = map;
                MainClass.output.MatchMade = true;
            }
        }

        private void DoList()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var commands = Tools.CommandFunctions.GetCommands("DevToolsParser");
            var cmdOutput = "";
            var nl = Environment.NewLine;

            foreach (var cmd in commands)
            {
                if (!cmdOutput.Equals(""))
                {
                    cmdOutput += nl;
                }
                cmdOutput += $"""{cmd}""";
            }

            if (!cmdOutput.Equals(""))
            {
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = cmdOutput;
            }
        }
    }
}