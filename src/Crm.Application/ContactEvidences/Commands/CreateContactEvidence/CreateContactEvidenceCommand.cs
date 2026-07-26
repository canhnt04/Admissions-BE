using Crm.Domain.Entities;
using MediatR;

namespace Crm.Application.ContactEvidences.Commands.CreateContactEvidence
{
    /// <summary>
    /// Command upload bằng chứng liên hệ KH.
    /// Sau khi submit, hệ thống sẽ update SlaTracking.IsContactMade = true (nếu là lần đầu).
    /// </summary>
    public class CreateContactEvidenceCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; set; }
        public Guid ConsultantId { get; set; }
        public ContactEvidenceType Type { get; set; }
        public string? FileUrl { get; set; }
        public string? Description { get; set; }
        public int? DurationSeconds { get; set; }
        public string? OldStatusValue { get; set; }
        public string? NewStatusValue { get; set; }
    }
}
