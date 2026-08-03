using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetContactEvidences
{
    public class GetContactEvidencesQuery : IRequest<Result<List<ContactEvidenceDto>>>
    {
        public Guid CustomerId { get; set; }
    }

    public class ContactEvidenceDto
    {
        public Guid Id { get; set; }
        public Guid ConsultantId { get; set; }
        public string ConsultantName { get; set; } = "Unknown";
        public string? FileUrl { get; set; }
        public string? Description { get; set; }
        public int? DurationSeconds { get; set; }
        public LeadStatus? LeadStatus { get; set; }
        public FollowStatus? FollowStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetContactEvidencesQueryHandler : IRequestHandler<GetContactEvidencesQuery, Result<List<ContactEvidenceDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;

        public GetContactEvidencesQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<Result<List<ContactEvidenceDto>>> Handle(GetContactEvidencesQuery request, CancellationToken cancellationToken)
        {
            var evidences = await _context.ContactEvidences
                .Where(e => e.CustomerId == request.CustomerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync(cancellationToken);

            var consultantIds = evidences.Select(e => e.ConsultantId).Distinct().ToList();
            var userNames = await _userGrpcClient.GetUserNamesAsync(consultantIds, cancellationToken);

            var result = evidences.Select(e => new ContactEvidenceDto
            {
                Id = e.Id,
                ConsultantId = e.ConsultantId,
                ConsultantName = userNames.GetValueOrDefault(e.ConsultantId, "Unknown"),
                FileUrl = e.FileUrl,
                Description = e.Description,
                DurationSeconds = e.DurationSeconds,
                LeadStatus = e.LeadStatus,
                FollowStatus = e.FollowStatus,
                CreatedAt = e.CreatedAt,
            }).ToList();

            return Result<List<ContactEvidenceDto>>.Success(result);
        }
    }
}
