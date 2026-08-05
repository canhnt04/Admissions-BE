using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace LeadAssignment.Domain.Entities
{
    /// <summary>
    /// Theo dõi SLA cho mỗi lần giao lead.
    /// Khi NV được giao lead, hệ thống tạo 1 SlaTracking với Deadline = Now + 30 phút.
    /// Background service kiểm tra mỗi phút: nếu quá Deadline mà chưa có ContactEvidence → vi phạm SLA → thu hồi lead.
    /// </summary>
    public class CustomerCareStatus
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public TrainingSystem? TrainingSystem { get; set; }

        public Guid? AssigneeId { get; set; }
        public LeadStatus? Status { get; set; }
        public FollowStatus? FollowStatus { get; set; }
        public DateTime? StatusDate { get; set; }
        public DateTime? ReportDate { get; set; }
        public string? Note { get; set; }
    }
}


