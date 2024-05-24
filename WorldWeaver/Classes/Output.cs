using System;
namespace WorldWeaver.Classes
{
    public class Output
    {
        public bool MatchMade { get; set; } = false;
        public string Value { get; set; } = "";
        public string OutputText { get; set; } = "";
        public bool FailedLogic { get; set; } = false;
        public bool Error { get; set; } = false;
        public bool ExitFlow { get; set; } = false;
    }
}
