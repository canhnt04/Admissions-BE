using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace LeadAssignment.Domain.Entities
{
    /// <summary>
    /// Bằng chứng liên hệ khách hàng.
    /// Mỗi lần NV tư vấn liên hệ KH phải upload bằng chứng (ghi âm, ghi chú, thay đổi status...).
    /// Dùng để xác nhận SLA compliance — nếu không có bằng chứng trong 30 phút, lead sẽ bị thu hồi.
    /// </summary>
    public class ContactEvidence
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với Customer
        public Guid CustomerId { get; set; }

        // NV tư vấn thực hiện liên hệ — chỉ lưu ID
        public Guid ConsultantId { get; set; }

        /// <summary>
        /// Loại bằng chứng (CallRecording, StatusChange, Note, Meeting, ZaloMessage, FacebookMessage)
        /// </summary>

        /// <summary>
        /// URL file ghi âm cuộc gọi (nếu Type = CallRecording)
        /// </summary>
        public string? FileUrl { get; set; }

        /// <summary>
        /// Mô tả nội dung tư vấn / ghi chú
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời lượng cuộc gọi tính bằng giây (nếu Type = CallRecording)
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Giá trị trạng thái cũ (nếu Type = StatusChange)
        /// </summary>
        public LeadStatus? LeadStatus { get; set; }
        public FollowStatus? FollowStatus { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
