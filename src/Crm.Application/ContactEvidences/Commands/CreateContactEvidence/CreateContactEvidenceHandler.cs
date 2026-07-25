using Crm.Application.Common.Interfaces;
using Crm.Application.Events;
using Crm.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.ContactEvidences.Commands.CreateContactEvidence
{
    public class CreateContactEvidenceHandler : IRequestHandler<CreateContactEvidenceCommand, Guid>
    {
        private readonly ICrmDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateContactEvidenceHandler(ICrmDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateContactEvidenceCommand request, CancellationToken cancellationToken)
        {
            // 1. Tạo bằng chứng liên hệ
            var evidence = new ContactEvidence
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ConsultantId = request.ConsultantId,
                Type = request.Type,
                FileUrl = request.FileUrl,
                Description = request.Description,
                DurationSeconds = request.DurationSeconds,
                OldStatusValue = request.OldStatusValue,
                NewStatusValue = request.NewStatusValue,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ContactEvidences.Add(evidence);

            // 2. Tìm SLA tracking đang active cho KH + NV này
            //    (IsContactMade = false, IsReassigned = false)
            var activeSla = await _context.SlaTrackings
                .FirstOrDefaultAsync(s =>
                    s.CustomerId == request.CustomerId &&
                    s.AssigneeId == request.ConsultantId &&
                    !s.IsContactMade &&
                    !s.IsReassigned,
                    cancellationToken);

            if (activeSla != null)
            {
                // Mark SLA as complied — NV đã liên hệ KH trong thời hạn
                activeSla.IsContactMade = true;
                activeSla.FirstContactAt = DateTime.UtcNow;
            }

            // 3. Ghi AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = Domain.Entities.Action.Insert,
                Detail = $"Upload bằng chứng liên hệ: {request.Type} cho KH {request.CustomerId}",
                RecordId = evidence.Id,
                RecordDesc = request.Type.ToString(),
                RecordEntity = RecordEntity.ContactEvidence,
                CreationDate = DateTime.UtcNow,
                UserId = request.ConsultantId,
            });

            await _context.SaveChangesAsync(cancellationToken);

            // 4. Publish event
            await _publishEndpoint.Publish(new ContactEvidenceSubmittedEvent
            {
                ContactEvidenceId = evidence.Id,
                CustomerId = request.CustomerId,
                ConsultantId = request.ConsultantId,
                EvidenceType = request.Type.ToString(),
                SubmittedAt = DateTime.UtcNow,
            }, cancellationToken);

            return evidence.Id;
        }
    }
}
