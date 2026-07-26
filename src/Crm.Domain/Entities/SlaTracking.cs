using System;

namespace Crm.Domain.Entities
{
    /// <summary>
    /// Theo dõi SLA cho mỗi lần giao lead.
    /// Khi NV được giao lead, hệ thống tạo 1 SlaTracking với Deadline = Now + 30 phút.
    /// Background service kiểm tra mỗi phút: nếu quá Deadline mà chưa có ContactEvidence → vi phạm SLA → thu hồi lead.
    /// </summary>
    public class SlaTracking
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // Khóa ngoại liên kết với User (NV đang giữ lead)
        public Guid AssigneeId { get; set; }
        public virtual User Assignee { get; set; }

        /// <summary>
        /// Thời điểm giao lead cho NV
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Hạn chót phải liên hệ KH (AssignedAt + 30 phút)
        /// </summary>
        public DateTime Deadline { get; set; }

        /// <summary>
        /// NV đã liên hệ KH chưa? (true khi có ít nhất 1 ContactEvidence)
        /// </summary>
        public bool IsContactMade { get; set; }

        /// <summary>
        /// Thời điểm liên hệ đầu tiên (null nếu chưa liên hệ)
        /// </summary>
        public DateTime? FirstContactAt { get; set; }

        /// <summary>
        /// Đã vi phạm SLA chưa? (true khi quá Deadline mà IsContactMade = false)
        /// </summary>
        public bool IsViolated { get; set; }

        /// <summary>
        /// Lead đã bị thu hồi & giao lại cho NV khác chưa?
        /// </summary>
        public bool IsReassigned { get; set; }

        /// <summary>
        /// Thời điểm thu hồi & giao lại
        /// </summary>
        public DateTime? ReassignedAt { get; set; }

        // Khóa ngoại liên kết với User (NV mới được giao lại)
        public Guid? ReassignedToId { get; set; }
        public virtual User? ReassignedTo { get; set; }
    }
}
