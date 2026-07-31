using System;

namespace Formal.Domain.Entities
{
    /// <summary>
    /// Thông báo in-app cho NV tư vấn.
    /// Gửi khi: giao lead mới, cảnh báo SLA sắp hết hạn, vi phạm SLA, lead được giao lại.
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }

        // Người nhận thông báo — chỉ lưu ID
        public Guid RecipientId { get; set; }

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
