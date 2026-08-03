using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Events;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.ContactEvidences.Commands.CreateContactEvidence
{
    public class CreateContactEvidenceHandler : IRequestHandler<CreateContactEvidenceCommand, Result<Guid>>
    {
        private readonly IContactEvidenceRepository _contactEvidenceRepository;
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateContactEvidenceHandler(
            IContactEvidenceRepository contactEvidenceRepository,
            ICustomerCareStatusRepository customerCareStatusRepository,
            IAuditLogRepository auditLogRepository,
            IAssignmentDbContext context,
            IPublishEndpoint publishEndpoint)
        {
            _contactEvidenceRepository = contactEvidenceRepository;
            _customerCareStatusRepository = customerCareStatusRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Guid>> Handle(CreateContactEvidenceCommand request, CancellationToken cancellationToken)
        {
            // 1. Tạo bằng chứng liên hệ
            var evidence = new ContactEvidence
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ConsultantId = request.ConsultantId,
                FileUrl = request.FileUrl,
                Description = request.Description,
                DurationSeconds = request.DurationSeconds,
                LeadStatus = request.LeadStatus,
                FollowStatus = request.FollowStatus,
                CreatedAt = DateTime.UtcNow,
            };

            _contactEvidenceRepository.Add(evidence);

            // 2. Tìm SLA tracking đang active cho KH + NV này
            //    (IsContactMade = false, IsReassigned = false)
            var activeSla = await _customerCareStatusRepository
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
                _customerCareStatusRepository.Update(activeSla);
            }

            // 3. Ghi AuditLog
            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Insert,
                Detail = $"Upload bằng chứng liên hệ cho KH {request.CustomerId}",
                RecordId = evidence.Id,
                RecordDesc = "Ghi chú tư vấn",
                RecordEntity = RecordEntity.CustomerNote,
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
                LeadStatus = (int?)request.LeadStatus,
                FollowStatus = (int?)request.FollowStatus,
                SubmittedAt = DateTime.UtcNow,
            }, cancellationToken);

            return Result<Guid>.Success(evidence.Id);
        }
    }
}
