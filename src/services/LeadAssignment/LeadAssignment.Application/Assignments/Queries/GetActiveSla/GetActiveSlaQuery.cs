using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetActiveSla
{
    public class GetActiveSlaQuery : IRequest<Result<List<ActiveSlaDto>>>
    {
        public TrainingSystem? TrainingSystem { get; set; }
        public Guid? ConsultantId { get; set; }
        public bool IncludeProcessed { get; set; }
    }

    public class ActiveSlaDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TrainingSystem { get; set; } = string.Empty;
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime Deadline { get; set; }
        public int RemainingMinutes { get; set; }
        public bool IsViolated { get; set; }
    }

    public class GetActiveSlaQueryHandler : IRequestHandler<GetActiveSlaQuery, Result<List<ActiveSlaDto>>>
    {
        private readonly IAssignmentDbContext _context;
        private readonly IUserGrpcClient _userGrpcClient;
        private readonly Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> _slaSettings;

        public GetActiveSlaQueryHandler(IAssignmentDbContext context, IUserGrpcClient userGrpcClient, Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> slaSettings)
        {
            _context = context;
            _userGrpcClient = userGrpcClient;
            _slaSettings = slaSettings;
        }

        public async Task<Result<List<ActiveSlaDto>>> Handle(GetActiveSlaQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CustomerCareStatuses.AsQueryable();

            if (!request.IncludeProcessed)
            {
                query = query.Where(s => s.Status == null || s.Status == LeadStatus.New);
            }

            if (request.TrainingSystem.HasValue)
                query = query.Where(s => s.TrainingSystem == request.TrainingSystem.Value);

            if (request.ConsultantId.HasValue)
                query = query.Where(s => s.AssigneeId == request.ConsultantId.Value);

            var rawSlaList = await query
                .OrderBy(s => s.StatusDate)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,
                    s.CustomerName,
                    s.TrainingSystem,
                    AssigneeId = s.AssigneeId,
                    StatusDate = s.StatusDate
                })
                .ToListAsync(cancellationToken);

            var assigneeIds = rawSlaList.Where(s => s.AssigneeId.HasValue).Select(s => s.AssigneeId!.Value).Distinct().ToList();
            var userInfos = await _userGrpcClient.GetUsersAsync(assigneeIds, cancellationToken);

            var now = Shared.Common.Helpers.TimeHelper.VietnamNow;
            
            var slaSettings = _slaSettings.Value;
            var managerIds = slaSettings.Managers.Values.ToList();
            if (slaSettings.DefaultManagerId != Guid.Empty) managerIds.Add(slaSettings.DefaultManagerId);

            var result = new List<ActiveSlaDto>();
            foreach (var s in rawSlaList)
            {
                var assigneeId = s.AssigneeId ?? Guid.Empty;
                bool isManager = managerIds.Contains(assigneeId);
                var baseSlaMins = slaSettings.SlaDeadlineMinutes;
                
                var assignedAt = s.StatusDate ?? now;
                var deadline = isManager ? DateTime.MaxValue : assignedAt.AddMinutes(baseSlaMins);

                result.Add(new ActiveSlaDto
                {
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    CustomerName = s.CustomerName,
                    TrainingSystem = s.TrainingSystem.ToString() ?? string.Empty,
                    AssigneeId = s.AssigneeId ?? Guid.Empty,
                    AssigneeName = s.AssigneeId.HasValue && userInfos.TryGetValue(s.AssigneeId.Value, out var info) && !string.IsNullOrEmpty(info.FullName) ? info.FullName : s.AssigneeId.HasValue ? $"Nhân viên ({s.AssigneeId.Value.ToString().Substring(0, 8)})" : "Chưa giao",
                    AssignedAt = assignedAt,
                    Deadline = deadline,
                    RemainingMinutes = isManager ? 999999 : (int)(deadline - now).TotalMinutes,
                    IsViolated = !isManager && now >= deadline
                });
            }



            return Result<List<ActiveSlaDto>>.Success(result);
        }
    }
}
