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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeadAssignment.Application.Assignments.Commands.AssignPendingLeads
{
    public class AssignPendingLeadsCommandHandler : IRequestHandler<AssignPendingLeadsCommand, Result<bool>>
    {
        private readonly ICustomerCareStatusRepository _customerCareStatusRepository;

        private readonly ICustomerAssignmentHistoryRepository _customerAssignmentHistoryRepository;
        private readonly Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> _slaSettings;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAssignmentDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly IEmailSender _emailSender;
        private readonly IUserGrpcClient _userGrpcClient;
        private readonly ILogger<AssignPendingLeadsCommandHandler> _logger;

        public AssignPendingLeadsCommandHandler(
            ICustomerCareStatusRepository customerCareStatusRepository,
            ICustomerAssignmentHistoryRepository customerAssignmentHistoryRepository,
            Microsoft.Extensions.Options.IOptions<LeadAssignment.Application.Common.Models.SlaSettings> slaSettings,
            IAuditLogRepository auditLogRepository,
            IAssignmentDbContext context,
            IPublishEndpoint publishEndpoint,
            IEmailSender emailSender,
            IUserGrpcClient userGrpcClient,
            ILogger<AssignPendingLeadsCommandHandler> logger)
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

        private Task<int> GetSlaDeadlineMinutesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_slaSettings.Value.SlaDeadlineMinutes);
        }

        public async Task<Result<bool>> Handle(AssignPendingLeadsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bắt đầu Assignment Engine cho nhánh {TrainingSystem}", request.TrainingSystem);

            var dbContext = (DbContext)_context;
            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Lấy tất cả khách hàng đang chờ (Pending)
                var pendingLeads = await _customerCareStatusRepository.Query()
                    .Where(c => c.AssigneeId == null && c.TrainingSystem == request.TrainingSystem)
                    .OrderBy(c => c.Id) // Hoặc theo CreatedAt nếu có
                    .ToListAsync(cancellationToken);

                if (!pendingLeads.Any())
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<bool>.Success(true);
                }

                // Lấy danh sách các nhân viên đang active (dựa vào AuditLog)
                var tenDaysAgo = Shared.Common.Helpers.TimeHelper.VietnamNow.AddDays(-10); // Chỉ lấy những ai có hoạt động gần đây
                
                var rawLogs = await _auditLogRepository.Query()
                    .Where(a => a.RecordEntity == RecordEntity.User && 
                           a.Action == LeadAssignment.Domain.Enums.Action.Update &&
                           (a.Detail.Contains("CHECK_IN") || a.Detail.Contains("CHECK_OUT")) &&
                           a.CreationDate > tenDaysAgo)
                    .ToListAsync(cancellationToken);

                var activeLogs = rawLogs
                    .GroupBy(a => a.UserId)
                    .Select(g => g.OrderByDescending(x => x.CreationDate).FirstOrDefault())
                    .ToList();

                var activeConsultantIds = activeLogs
                    .Where(l => l != null && 
                           l.Detail != null && 
                           l.Detail.ToLower().Contains("check_in") && 
                           l.Detail.ToLower().Contains($"nhánh {request.TrainingSystem.ToString().ToLower()}"))
                    .Select(l => l.UserId)
                    .ToList();

                if (!activeConsultantIds.Any())
                {
                    _logger.LogWarning("Không tìm được NV nào đang rảnh trong queue cho nhánh {TrainingSystem}. Cần chờ nhân viên check-in.", request.TrainingSystem);
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<bool>.Success(false);
                }

                // Với mỗi consultant, tính CurrentLoad và LastAssignedAt
                var queueStates = new List<ConsultantQueueState>();
                var consultantNames = await _userGrpcClient.GetUserFullNamesAsync(activeConsultantIds, cancellationToken);

                foreach (var cid in activeConsultantIds)
                {
                    var currentLoad = await _customerCareStatusRepository.Query()
                        .CountAsync(c => c.AssigneeId == cid && c.Status == LeadStatus.New && c.TrainingSystem == request.TrainingSystem, cancellationToken);
                        
                    // Nếu lớn hơn 10 (MaxLoad mặc định) thì không giao
                    if (currentLoad >= 10) continue;

                    var lastAssignment = await _customerAssignmentHistoryRepository.Query()
                        .Where(h => h.AssigneeId == cid)
                        .OrderByDescending(h => h.AssignmentDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    queueStates.Add(new ConsultantQueueState
                    {
                        ConsultantId = cid,
                        CurrentLoad = currentLoad,
                        LastAssignedAt = lastAssignment?.AssignmentDate ?? DateTime.MinValue
                    });
                }

                if (!queueStates.Any())
                {
                    _logger.LogWarning("Tất cả NV đang active đều đã full load (>10) cho nhánh {TrainingSystem}.", request.TrainingSystem);
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<bool>.Success(false);
                }

                var now = Shared.Common.Helpers.TimeHelper.VietnamNow;

                int assignedCount = 0;

                foreach (var lead in pendingLeads)
                {
                    // Lọc lại những người chưa bị đầy
                    var availableQueues = queueStates.Where(q => q.CurrentLoad < 10).ToList();
                    if (!availableQueues.Any())
                    {
                        break; // Hết chỗ trống
                    }

                    // Chọn người có LastAssignedAt cũ nhất
                    var selectedQueue = availableQueues.OrderBy(q => q.LastAssignedAt).First();

                    var consultantName = consultantNames.TryGetValue(selectedQueue.ConsultantId, out var name) && !string.IsNullOrEmpty(name)
                        ? name 
                        : $"Nhân viên ({selectedQueue.ConsultantId.ToString()[..8]})";
                    
                    int multiplier = Math.Min(_slaSettings.Value.MaxSlaMultiplier, Math.Max(1, selectedQueue.CurrentLoad + 1));
                    var dynamicSlaMinutes = _slaSettings.Value.SlaDeadlineMinutes * multiplier;
                    var deadline = now.AddMinutes(dynamicSlaMinutes);

                    // Cập nhật trạng thái lead
                    lead.AssigneeId = selectedQueue.ConsultantId;
                    lead.Status = LeadStatus.New;
                    lead.StatusDate = now; 
                    
                    _customerCareStatusRepository.Update(lead);

                    // Ghi log lịch sử
                    _customerAssignmentHistoryRepository.Add(new CustomerAssignmentHistory
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = lead.CustomerId,
                        AssigneeId = selectedQueue.ConsultantId,
                        AssignedById = Guid.Empty, // Hệ thống tự động
                        AssignmentDate = now
                    });

                    // Cập nhật state in memory
                    selectedQueue.CurrentLoad += 1;
                    selectedQueue.LastAssignedAt = now;

                    assignedCount++;

                    // Publish sự kiện ra ngoài
                    await _publishEndpoint.Publish(new LeadAssignedEvent
                    {
                        CustomerId = lead.CustomerId,
                        CustomerName = lead.CustomerName,
                        AssigneeId = selectedQueue.ConsultantId,
                        AssigneeName = consultantName,
                        AssignedById = Guid.Empty,
                        Reason = AssignmentReason.NewLead,
                        AssignedAt = now,
                        SlaDeadline = deadline,
                    }, cancellationToken);


                    // Gửi email nhắc nhở
                    await _emailSender.SendEmailAsync(
                        $"{selectedQueue.ConsultantId}@system.local",
                        $"Bạn được giao lead mới: {lead.CustomerName}",
                        $"<p>Hệ thống vừa tự động giao khách hàng <b>{lead.CustomerName}</b> cho bạn. Vui lòng liên hệ với khách hàng trước thời hạn {deadline:HH:mm:ss dd/MM/yyyy}.</p>",
                        cancellationToken);

                    _logger.LogInformation("Tự động giao KH {CustomerName} ({CustomerId}) cho NV {ConsultantName}", lead.CustomerName, lead.CustomerId, consultantName);
                    

                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Assignment Engine hoàn tất, đã giao {AssignedCount}/{TotalPending} lead.", assignedCount, pendingLeads.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình chạy Assignment Engine");
                await transaction.RollbackAsync(cancellationToken);
                return Result<bool>.Failure(new Shared.Common.Error(500, "AssignmentEngineFailed", ex.Message));
            }
        }
        
        private class ConsultantQueueState
        {
            public Guid ConsultantId { get; set; }
            public int CurrentLoad { get; set; }
            public DateTime LastAssignedAt { get; set; }
        }
    }
}
