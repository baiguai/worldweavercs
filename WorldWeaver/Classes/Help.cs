using WorldWeaver.Tools;

namespace WorldWeaver.Classes
{
    public class Help
    {
        public string TopicId { get; set; } = "";
        public string Title { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Related { get; set; } = new List<string>();
        public string Article { get; set; } = "";
    }
}
