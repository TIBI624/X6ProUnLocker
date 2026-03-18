namespace X6ProUnLocker.Core
{
    public class SystemIssue
    {
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Solution { get; set; } = "";
        public bool AutoFixable { get; set; }
    }
}