using System;

namespace Crm.Domain.Entities
{
    /// <summary>
    /// Queue giao khách tự động (Round-Robin).
    /// Mỗi nhánh đào tạo (Chính Quy / Ngắn Hạn / Lái Xe) có 1 queue riêng.
    /// NV được xếp theo OrderIndex; khi có lead mới, hệ thống giao cho NV có LastAssignedAt cũ nhất.
    /// </summary>
    public class AssignmentQueue
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Nhánh đào tạo mà queue này phục vụ
        /// </summary>
        public TrainingSystem TrainingSystem { get; set; }

        // Khóa ngoại liên kết với User (NV tư vấn)
        public Guid ConsultantId { get; set; }
        public virtual User Consultant { get; set; }

        /// <summary>
        /// Thứ tự trong queue (dùng cho Round-Robin)
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Số lead đang giữ hiện tại
        /// </summary>
        public int CurrentLoad { get; set; }

        /// <summary>
        /// Giới hạn lead tối đa mà NV này có thể nhận
        /// </summary>
        public int MaxLoad { get; set; }

        /// <summary>
        /// NV có đang nhận lead mới không (false = tạm dừng nhận lead)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời điểm gần nhất NV được giao lead (dùng để xác định Round-Robin order)
        /// </summary>
        public DateTime? LastAssignedAt { get; set; }
    }
}
