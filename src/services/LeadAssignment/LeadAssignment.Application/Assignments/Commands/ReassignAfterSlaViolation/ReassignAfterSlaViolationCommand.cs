using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Application.Events;
using LeadAssignment.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.ReassignAfterSlaViolation
{
    public class ReassignAfterSlaViolationCommand : IRequest<Result<Guid?>>
    {
        public Guid CustomerId { get; set; }
        public Guid ViolatedAssigneeId { get; set; }
    }

    public class ReassignAfterSlaViolationCommandHandler : IRequestHandler<ReassignAfterSlaViolationCommand, Result<Guid?>>
    {
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;

        private readonly ICustomerAssignmentHistoryRepository _customerAssignmentHistoryRepository;
        private readonly Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> _slaSettings;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly IEmailSender _emailSender;
        private readonly IUserGrpcClient _userGrpcClient;
        private readonly ILogger<ReassignAfterSlaViolationCommandHandler> _logger;

        public ReassignAfterSlaViolationCommandHandler(
            ICustomerCareStatusRepository customerCareStatusRepository,
            ICustomerAssignmentHistoryRepository customerAssignmentHistoryRepository,
            Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> slaSettings,
            IAuditLogRepository auditLogRepository,
            IAssignmentDbContext context,
            IPublishEndpoint publishEndpoint,
            IEmailSender emailSender,
            IUserGrpcClient userGrpcClient,
            ILogger<ReassignAfterSlaViolationCommandHandler> logger)
        {
            _customerCareStatusRepository = customerCareStatusRepository;
            _customerAssignmentHistoryRepository = customerAssignmentHistoryRepository;
            _slaSettings = slaSettings;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _emailSender = emailSender;
            _userGrpcClient = userGrpcClient;
            _logger = logger;
        }

        private Task<int> GetSlaDeadlineMinutesAsync(CancellationToken cancellationToken, bool isAdmin = false)
        {
            return Task.FromResult(isAdmin ? _slaSettings.Value.AdminSlaDeadlineMinutes : _slaSettings.Value.SlaDeadlineMinutes);
        }

        private Task<Guid?> GetManagerIdAsync(TrainingSystem? trainingSystem, CancellationToken cancellationToken)
        {
            if (trainingSystem.HasValue && _slaSettings.Value.Managers.TryGetValue(trainingSystem.Value.ToString(), out var managerId) && managerId != Guid.Empty)
            {
                return Task.FromResult<Guid?>(managerId);
            }
            return Task.FromResult<Guid?>(_slaSettings.Value.DefaultManagerId != Guid.Empty ? _slaSettings.Value.DefaultManagerId : null);
        }

        public async Task<Result<Guid?>> Handle(ReassignAfterSlaViolationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleInner(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong ReassignAfterSlaViolationCommand cho Customer {CustomerId}", request.CustomerId);
                throw;
            }
        }
        private async Task<Result<Guid?>> HandleInner(ReassignAfterSlaViolationCommand request, CancellationToken cancellationToken)
        {
            var latestStatus = await _customerCareStatusRepository.Query()
                .Where(s => s.CustomerId == request.CustomerId && s.AssigneeId == request.ViolatedAssigneeId)
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestStatus == null) return Result<Guid?>.Success(null);

            var customerName = latestStatus.CustomerName;
            var trainingSystem = latestStatus.TrainingSystem;
            var now = Shared.Common.Helpers.TimeHelper.VietnamNow;



            // Đếm số lần khách hàng này đã được giao cho các nhân viên
            var pastAssignments = await _customerAssignmentHistoryRepository.Query()
                .Where(h => h.CustomerId == request.CustomerId)
                .OrderBy(h => h.AssignmentDate)
                .ToListAsync(cancellationToken);
            
            var pastAssigneeIds = pastAssignments.Select(x => x.AssigneeId).Distinct().ToList();
            if (!pastAssigneeIds.Contains(request.ViolatedAssigneeId))
            {
                pastAssigneeIds.Add(request.ViolatedAssigneeId);
                
                // Ghi nhận lịch sử cũ nếu trước đó bị thiếu, để các tiến trình khác (như AssignPendingLeads) còn biết mà loại trừ
                _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    AssigneeId = request.ViolatedAssigneeId,
                    AssignedById = Guid.Empty,
                    AssignmentDate = latestStatus.StatusDate ?? now
                });
            }
            
            var assignmentCount = pastAssignments.Count;
            var isThreeStrikes = assignmentCount >= 3; // Mọi lần từ 3 trở lên đều tính là Three Strikes để cảnh báo

            Guid? nextAssigneeId = null;

            if (isThreeStrikes)
            {
                var managerId = await GetManagerIdAsync(trainingSystem, cancellationToken);
                if (managerId.HasValue && managerId.Value != request.ViolatedAssigneeId)
                {
                    nextAssigneeId = managerId.Value;
                    
                    // Pause routing for violating consultants by logging CHECK_OUT event
                    
                    foreach(var cid in pastAssigneeIds)
                    {
                        _auditLogRepository.Add(new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            Action = LeadAssignment.Domain.Enums.Action.Update,
                            Detail = $"[CHECK_OUT] Tạm ngưng chia lead do vi phạm SLA 3 lần liên tiếp với khách hàng [{customerName}]",
                            RecordId = cid,
                            RecordDesc = cid.ToString(),
                            RecordEntity = RecordEntity.User,
                            CreationDate = now,
                            UserId = Guid.Empty, // System
                        });
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Khách hàng {CustomerId} vi phạm SLA 3 lần nhưng chưa cấu hình Manager. Sẽ tiếp tục vòng lặp Round-Robin.",
                        request.CustomerId);
                }
            }

            if (nextAssigneeId == null)
            {
                // Find next active consultant
                var tenDaysAgo = Shared.Common.Helpers.TimeHelper.VietnamNow.AddDays(-10);
                var rawLogs = await _auditLogRepository.Query()
                    .Where(a => a.RecordEntity == RecordEntity.User && 
                           a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                           (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")) &&
                           a.CreationDate > tenDaysAgo)
                    .ToListAsync(cancellationToken);

                var activeLogs = rawLogs
                    .GroupBy(a => a.RecordId)
                    .Select(g => g.OrderByDescending(x => x.CreationDate).FirstOrDefault())
                    .ToList();

                var activeConsultantIds = activeLogs
                    .Where(l => l != null && l.Detail.Contains("CHECK_IN") && !pastAssigneeIds.Contains(l.RecordId))
                    .Select(l => l.RecordId)
                    .ToList();

                if (!activeConsultantIds.Any())
                {
                    _logger.LogWarning(
                        "Không tìm được nhân viên active thay thế cho khách hàng {CustomerId}. Chuyển cho Manager.",
                        request.CustomerId);
                    
                    var fallbackManager = await GetManagerIdAsync(trainingSystem, cancellationToken);
                    if (fallbackManager.HasValue && fallbackManager.Value != request.ViolatedAssigneeId)
                    {
                        nextAssigneeId = fallbackManager.Value;
                    }
                    else
                    {
                        _logger.LogWarning("Không cấu hình Manager fallback hoặc Manager chính là người vi phạm. Khách hàng được trả về cho hệ thống (Unassigned).");
                        nextAssigneeId = null;
                    }
                }
                else
                {
                    // Choose the one with oldest assignment date
                    Guid? chosenId = null;
                    DateTime oldestAssignment = DateTime.MaxValue;

                    foreach (var cid in activeConsultantIds)
                    {
                        var lastAssignment = await _customerAssignmentHistoryRepository.Query()
                            .Where(h => h.AssigneeId == cid)
                            .OrderByDescending(h => h.AssignmentDate)
                            .FirstOrDefaultAsync(cancellationToken);

                        var date = lastAssignment?.AssignmentDate ?? DateTime.MinValue;
                        if (date < oldestAssignment)
                        {
                            oldestAssignment = date;
                            chosenId = cid;
                        }
                    }

                    nextAssigneeId = chosenId ?? activeConsultantIds.First();
                }
            }

            // Resolve tên NV mới qua gRPC (batch call)
            var userIdsToResolve = new System.Collections.Generic.List<Guid> { request.ViolatedAssigneeId };
            if (nextAssigneeId.HasValue) userIdsToResolve.Add(nextAssigneeId.Value);

            var resolvedUserInfos = await _userGrpcClient.GetUsersAsync(
                userIdsToResolve.ToArray(), cancellationToken);
            var newConsultantName = nextAssigneeId.HasValue && resolvedUserInfos.TryGetValue(nextAssigneeId.Value, out var ni) ? ni.FullName : "Hệ thống";
            var nextAssigneeEmail = nextAssigneeId.HasValue && resolvedUserInfos.TryGetValue(nextAssigneeId.Value, out var ne) ? ne.Email : string.Empty;
            var violatedAssigneeEmail = resolvedUserInfos.TryGetValue(request.ViolatedAssigneeId, out var ve) ? ve.Email : string.Empty;

            if (isThreeStrikes)
            {
                // Resolve names of violating consultants for notification
                var violatingAssigneeIds = pastAssignments.Select(x => x.AssigneeId).Distinct().ToList();
                var violatingUserInfos = await _userGrpcClient.GetUsersAsync(violatingAssigneeIds, cancellationToken);
                var namesStr = string.Join(", ", violatingAssigneeIds.Select(id => violatingUserInfos.TryGetValue(id, out var v) ? v.FullName : string.Empty));

                await _emailSender.SendEmailAsync(
                    nextAssigneeEmail,
                    "CẢNH BÁO ESCALATION: Khách hàng vi phạm SLA nhiều lần",
                    $"<p>Lead <b>{customerName}</b> đã bị vi phạm từ nhân viên [{namesStr}]. Vui lòng xử lý gấp.</p>",
                    cancellationToken);
            }

            int currentLoad = nextAssigneeId.HasValue ? await _customerCareStatusRepository.Query()
                .CountAsync(c => c.AssigneeId == nextAssigneeId.Value && c.Status == LeadStatus.New && c.TrainingSystem == trainingSystem, cancellationToken) : 0;

            var slaMinutes = await GetSlaDeadlineMinutesAsync(cancellationToken, isAdmin: false); // We don't care about isAdmin here anymore, handled below
            var deadline = isThreeStrikes ? DateTime.MaxValue : now.AddMinutes(slaMinutes);

            // Instead of marking IsReassigned, we just update the existing CustomerCareStatus
            latestStatus.AssigneeId = nextAssigneeId;
            latestStatus.StatusDate = now;
            latestStatus.Status = LeadStatus.New; // Reset status so SLA monitor starts tracking anew
            _customerCareStatusRepository.Update(latestStatus);

            _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AssigneeId = nextAssigneeId ?? Guid.Empty,
                AssignedById = request.ViolatedAssigneeId,
                AssignmentDate = now
            });

            _auditLogRepository.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = LeadAssignment.Domain.Enums.Action.Assign,
                Detail = $"SLA Violation: Thu hồi KH [{customerName}] từ NV {request.ViolatedAssigneeId}, giao lại cho [{newConsultantName}]",
                RecordId = request.CustomerId,
                RecordDesc = customerName,
                RecordEntity = RecordEntity.Customer,
                CreationDate = now,
                UserId = request.ViolatedAssigneeId,
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new LeadAssignedEvent
            {
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                AssigneeId = nextAssigneeId ?? Guid.Empty,
                AssigneeName = newConsultantName,
                AssignedById = request.ViolatedAssigneeId,
                Reason = AssignmentReason.SlaViolation,
                AssignedAt = now,
                SlaDeadline = deadline,
            }, cancellationToken);



            await _emailSender.SendEmailAsync(
                violatedAssigneeEmail,
                "CẢNH BÁO: Vi phạm SLA Khách hàng",
                $"<p>Bạn đã vi phạm SLA khi không liên hệ khách hàng {customerName} trong thời gian quy định. Lead này đã bị hệ thống thu hồi tự động.</p>",
                cancellationToken);

            _logger.LogInformation(
                "SLA Reassigned: KH {CustomerName} ({CustomerId}) từ NV {OldAssignee} sang {NewAssignee}",
                customerName, request.CustomerId, request.ViolatedAssigneeId, newConsultantName);

            return Result<Guid?>.Success(nextAssigneeId.Value);
        }
    }
}
