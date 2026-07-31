namespace LeadAssignment.Application.Events
{
    /// <summary>
    /// Event phát ra khi NV vi phạm SLA (quá hạn chưa liên hệ KH).
    /// </summary>
    public class SlaViolationEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid ViolatedAssigneeId { get; set; }
        public string ViolatedAssigneeName { get; set; } = string.Empty;
        public Guid SlaTrackingId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime ViolatedAt { get; set; }
    }
}
