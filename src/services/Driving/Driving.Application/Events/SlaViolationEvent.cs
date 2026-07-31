namespace Driving.Application.Events
{
    /// <summary>
    /// Event phát ra khi NV vi phạm SLA (quá 30 phút chưa liên hệ KH).
    /// SlaViolationConsumer sẽ lắng nghe event này để thu hồi lead và giao cho NV tiếp theo.
    /// </summary>
    public class SlaViolationEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }

        /// <summary>
        /// NV bị vi phạm SLA
        /// </summary>
        public Guid ViolatedAssigneeId { get; set; }
        public string ViolatedAssigneeName { get; set; }

        public Guid SlaTrackingId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime ViolatedAt { get; set; }
    }
}
