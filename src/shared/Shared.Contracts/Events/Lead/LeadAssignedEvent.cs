using System;

namespace Shared.Contracts.Events.Lead
{
    public class LeadAssignedEvent
    {
        public Guid LeadId { get; set; }
        public Guid AssigneeId { get; set; }
        public Guid AssignedBy { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
