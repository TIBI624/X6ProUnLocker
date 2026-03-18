namespace X6ProUnLocker.Core
{
    public class AutoStartEntry
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string Type { get; set; } = "";
        public string Location { get; set; } = "";
        public bool IsEnabled { get; set; }
        public string Description { get; set; } = "";
    }
}