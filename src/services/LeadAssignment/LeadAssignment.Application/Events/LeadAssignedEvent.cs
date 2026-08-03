using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Domain.Entities;

namespace LeadAssignment.Application.Events
{
    /// <summary>
    /// Event phát ra khi lead được giao cho NV (cả tự động lẫn thủ công).
    /// </summary>
    public class LeadAssignedEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public Guid AssignedById { get; set; }
        public AssignmentReason Reason { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime SlaDeadline { get; set; }
    }
}
