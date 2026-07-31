using Driving.Domain.Entities;

namespace Driving.Application.Events
{
    /// <summary>
    /// Event phát ra khi lead được giao cho NV (cả tự động lẫn thủ công).
    /// </summary>
    public class LeadAssignedEvent
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public Guid AssignedById { get; set; }
        public AssignmentReason Reason { get; set; }
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Hạn chót phải liên hệ KH (30 phút kể từ AssignedAt)
        /// </summary>
        public DateTime SlaDeadline { get; set; }
    }
}
