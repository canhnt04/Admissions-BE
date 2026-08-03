using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
namespace LeadAssignment.Application.Events
{
    /// <summary>
    /// Event phát ra khi NV submit bằng chứng liên hệ (ghi âm, ghi chú, thay đổi status...).
    /// Dùng để update SlaTracking.IsContactMade = true.
    /// </summary>
    public class ContactEvidenceSubmittedEvent
    {
        public Guid ContactEvidenceId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ConsultantId { get; set; }
        public int? LeadStatus { get; set; }
        public int? FollowStatus { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}

