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
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoList"))
                {
                    DoList();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoMap"))
                {
                    DoMap();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoRoom"))
                {
                    DoRoom();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoMacroStart"))
                {
                    DoMacroStart();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoMacroStop"))
                {
                    DoMacroStop();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoDescMacro"))
                {
                    DoDescMacro();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoMacroList"))
                {
                    DoMacroList();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoDeleteMacro"))
                {
                    DoDeleteMacro();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoNoteSearch"))
                {
                    DoNoteSearch();
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoRunMacro"))
                {
                    if (!MainClass.macro.IsRunning)
                    {
                        DoRunMacro(false);
                    }
                    else
                    {
                        DoRunMacro(MainClass.macro.DoTests);
                    }
                    return;
                }
                if (!MainClass.output.MatchMade && method.Equals("DoRunMacroTestMode"))
                {
                    if (!MainClass.macro.IsRunning)
                    {
                        DoRunMacro(true);
                    }
                    else
                    {
                        DoRunMacro(MainClass.macro.DoTests);
                    }
                    return;
                }
            }
        }

        private void DoNoteSearch()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }

            var searchString = MainClass.userInput.Replace("_notesearch ", "");
            var elemDb = new DataManagement.GameLogic.Element();

            var res = elemDb.SearchAdminNotes(searchString);

            foreach (var itm in res)
            {
                if (!MainClass.output.OutputText.Equals(""))
                {
                    MainClass.output.OutputText += Environment.NewLine;
                }
                MainClass.output.OutputText += $"[{itm.Parent}]:{Environment.NewLine}{itm.Output}{Environment.NewLine}";
            }

            if (!MainClass.output.OutputText.Equals(""))
            {
                MainClass.output.MatchMade = true;
            }
            else
            {
                MainClass.output.OutputText = $"No notes found for search string: {searchString}";
                MainClass.output.MatchMade = true;
            }
        }

        private void DoAdminLogin(string userInput)
        {
            var elemDb = new DataManagement.GameLogic.Element();

            var pwdElem = elemDb.GetElementChildByTag(GameCache.Game.ElementKey, "!_adminpass");
            if (pwdElem == null || pwdElem.ElementKey.Equals(""))
            {
                return;
            }

            if (userInput.Equals(pwdElem.Output))
            {
                MainClass.adminEnabled = true;
                MainClass.output.MatchMade = true;
                MainClass.output.OutputText = "Admin mode enabled. Use _adminls to view the possible admin commands. Or consult the Admin Help System.";
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

            foreach (var room in rooms)
            {
                map += DoRoom(room, map);
            }
        }

        private void DoRoom()
        {
            DoRoom(Cache.RoomCache.Room, "");
        }
        private string DoRoom(Classes.Element room, string map)
        {
            var elemDb = new DataManagement.GameLogic.Element();
            var nl = Environment.NewLine;
            var space = $"{Environment.NewLine}{Environment.NewLine}";
            var indent = "   ";

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


            if (!map.Equals(""))
            {
                MainClass.output.OutputText = map;
                MainClass.output.MatchMade = true;
            }

            return map;
        }

        private void DoList()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var commands = Tools.CommandFunctions.GetCommandsAndDescription("DevToolsParser");
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

        private void DoMacroStart()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var macroName = MainClass.userInput.Replace("_recordon ", "");
            if (MainClass.userInput.Equals("_recordon"))
            {
                macroName = "";
            }

            if (macroName.Equals(""))
            {
                MainClass.output.OutputText = $"Be sure to specify a single word (File friendly) name for your macro.{Environment.NewLine}The shorter the name the better.";
                MainClass.output.MatchMade = true;
                return;
            }

            MainClass.macro = new Classes.Macro()
            {
                MacroName = macroName.FileSafe(),
                IsRecording = true
            };
            MainClass.output.OutputText = $"Recording macro: {MainClass.macro.MacroName}";
            MainClass.output.MatchMade = true;
        }

        private void DoMacroStop()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            MainClass.macro.IsRecording = false;
            var gameDb = new DataManagement.GameLogic.Game();
            var macroContents = "";
            var macroDir = $"Config/Macros/{MainClass.gameDb}";

            EnsureMacroDir(macroDir);

            var macroFile = $"{macroDir}/{MainClass.macro.MacroName}";

            try
            {
                File.Delete(macroFile);
            }
            catch (Exception)
            { }

            foreach (var st in MainClass.macro.MacroSteps)
            {
                if (!macroContents.Equals(""))
                {
                    macroContents += Environment.NewLine;
                }
                macroContents += st;
            }

            File.WriteAllText(macroFile, macroContents);

            MainClass.output.OutputText = $"The macro steps have been stored in: {macroFile}";
            MainClass.output.MatchMade = true;
        }

        private void DoRunMacro(bool testMode)
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var macroDir = $"Config/Macros/{MainClass.gameDb}";
            var macroName = "";

            EnsureMacroDir(macroDir);

            // Handle both testing mode and non-testing mode
            macroName = MainClass.userInput.Replace("_macro! ", "");
            macroName = macroName.Replace("_macro ", "");

            if (!File.Exists($"{macroDir}/{macroName.FileSafe()}"))
            {
                return;
            }

            if (!MainClass.macro.IsRunning)
            {
                MainClass.testResults.Clear();
                MainClass.macro.InitialMacroName = macroName;
                MainClass.macro.IsRunning = true;
                MainClass.macro.DoTests = testMode;
            }

            var lines = File.ReadAllLines($"{macroDir}/{macroName}").ToList();
            foreach (var line in lines)
            {
                MainClass.RunTheParsers(line);
                if (MainClass.output.MatchMade)
                {
                    // MainClass.HandleTheFight();
                }
            }

            if (macroName.Equals(MainClass.macro.InitialMacroName))
            {
                MainClass.macro.IsRunning = false;
                if (MainClass.testResults.Count > 0)
                {
                    MainClass.output.OutputText = Tools.OutputProcessor.DisplayTestResults();
                    MainClass.output.MatchMade = true;
                }
            }
        }

        private void DoDescMacro()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var macroDir = $"Config/Macros/{MainClass.gameDb}";
            var macDesc = "";

            EnsureMacroDir(macroDir);

            var macroName = MainClass.userInput.Replace("_macrodesc ", "");
            if (!File.Exists($"{macroDir}/{macroName.FileSafe()}"))
            {
                return;
            }

            var lines = File.ReadAllLines($"{macroDir}/{macroName}").ToList();
            foreach (var line in lines)
            {
                if (!macDesc.Equals(""))
                {
                    macDesc += Environment.NewLine;
                }
                macDesc += line;
            }

            MainClass.output.OutputText = macDesc;
            MainClass.output.MatchMade = true;
        }

        private void DoDeleteMacro()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var macroDir = $"Config/Macros/{MainClass.gameDb}";

            EnsureMacroDir(macroDir);

            var macroName = MainClass.userInput.Replace("_macrodel ", "");
            if (!File.Exists($"{macroDir}/{macroName.FileSafe()}"))
            {
                return;
            }

            File.Delete($"{macroDir}/{macroName.FileSafe()}");
            MainClass.output.OutputText = $"The macro {macroName} has been deleted.";
            MainClass.output.MatchMade = true;
        }

        private void DoMacroList()
        {
            if (!MainClass.adminEnabled)
            {
                return;
            }
            var macroDir = $"Config/Macros/{MainClass.gameDb}";

            EnsureMacroDir(macroDir);

            MainClass.output.OutputText = "";

            foreach (var f in Directory.GetFiles(macroDir))
            {
                if (!MainClass.output.OutputText.Equals(""))
                {
                    MainClass.output.OutputText += Environment.NewLine;
                }
                MainClass.output.OutputText += Path.GetFileName(f);
            }

            if (MainClass.output.OutputText.Equals(""))
            {
                MainClass.output.OutputText = "No macros have been recorded yet.";
            }

            MainClass.output.MatchMade = true;
        }

        private void EnsureMacroDir(string macroDir)
        {
            if (!Directory.Exists(macroDir))
            {
                Directory.CreateDirectory(macroDir);
            }
        }
    }
}