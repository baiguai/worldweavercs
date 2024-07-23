namespace WorldWeaver.Classes
{
    public class ElementProcessingProps
    {
        public bool FailedLogicCheck { get; set; } = false;
        public bool ExitChildLoop { get; set; } = false;
        public bool ExitProcLoop { get; set; } = false;
        public bool ExitAllLoops { get; set; } = false;
        public bool LoopThroughChildren { get; set; } = true;
        public bool HasChanges { get; set; } = false;

        public bool ReturnMessage { get; set; } = true;
    }
}
