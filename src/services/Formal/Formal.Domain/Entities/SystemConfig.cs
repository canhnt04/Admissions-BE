namespace Formal.Domain.Entities
{
    public class SystemConfig
    {
        public string Id { get; set; } // Key (e.g., "SlaDeadlineMinutes", "DefaultManagerId")
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
