using System;
using System.Collections.Generic;
using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class Element
    {
        public string ElementType { get; set; } = "";
        public string ElementKey { get; set; } = "";
        public string Name { get; set; } = "";
        public string ParentKey { get; set; } = "";
        public string Syntax { get; set; } = "";
        public string Logic { get; set; } = "";
        public string Repeat { get; set; } = "repeat";
        public int RepeatIndex { get; set; } = 0;
        public string Output { get; set; } = "";
        public string Tags { get; set; } = "";
        public string Active { get; set; } = "true";
        public List<Element> Children { get; set; } = new List<Element>();
        public int Sort { get; set; } = 1;
        public string CreateDate { get; set; } = DateTime.Now.FormatDate();
        public string UpdateDate { get; set; } = DateTime.Now.FormatDate();

        // Processing tools
        public bool ExitChildLoop {get; set; } = false;
        public bool ExitProcLoop { get; set; } = false;
        public bool ExitAllLoops { get; set; } = false;
        public bool LoopThroughChildren { get; set; } = true;

        public bool ReturnMessage { get; set; } = true;


        public void ParseElement(bool isEntering = false)
        {
            var elemPars = new Parsers.Elements.Element();
            var procsList = Tools.ProcFunctions.GetProcessStepsByType(this.ElementType);
            foreach (var procs in procsList)
            {
                elemPars.ParseElement(this, procs, isEntering);
            }
        }

        public List<ElementProc> GetProcs()
        {
            return Tools.ProcFunctions.GetProcessStepsByType(this.ElementType);
        }

        internal List<Classes.Element> GetChildren(bool includeAttributes = false)
        {
            if (this.Children.Count < 1)
            {
                var elemDb = new DataManagement.GameLogic.Element();
                this.Children = elemDb.GetElementChildren(this.ElementKey);

                if (!includeAttributes)
                {
                    this.Children = this.Children.Where(c => c.ElementType != "attribute").ToList();
                }

                return this.Children;
            }
            else
            {
                return this.Children;
            }
        }
    }
}
