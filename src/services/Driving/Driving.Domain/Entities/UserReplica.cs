using System;

namespace Driving.Domain.Entities
{
    /// <summary>
    /// Bản sao tối giản của User — lưu trong CRM databases (Formal/ShortTerm/Driving)
    /// để JOIN nội bộ mà không cần gọi API chéo sang Auth service.
    /// Dữ liệu được đồng bộ qua RabbitMQ (Event Driven).
    /// </summary>
    public class UserReplica
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string? Mobile { get; set; }
        public Role Role { get; set; }
        public Guid? TeamId { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời điểm đồng bộ gần nhất từ Auth service
        /// </summary>
        public DateTime LastSyncedAt { get; set; }
    }
}
