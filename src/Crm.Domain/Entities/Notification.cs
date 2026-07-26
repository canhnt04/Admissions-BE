using System;

namespace Crm.Domain.Entities
{
    /// <summary>
    /// Thông báo in-app cho NV tư vấn.
    /// Gửi khi: giao lead mới, cảnh báo SLA sắp hết hạn, vi phạm SLA, lead được giao lại.
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với User (người nhận thông báo)
        public Guid RecipientId { get; set; }
        public virtual User Recipient { get; set; }

        /// <summary>
        /// Loại thông báo
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Tiêu đề thông báo
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Nội dung chi tiết
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ID bản ghi liên quan (CustomerId, SlaTrackingId, v.v.)
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Đã đọc chưa
        /// </summary>
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
