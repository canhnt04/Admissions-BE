using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
namespace LeadAssignment.Domain.Entities
{
    public class SystemConfig
    {
        public string Id { get; set; } // Key (e.g., "SlaDeadlineMinutes", "DefaultManagerId")
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
