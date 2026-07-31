using LeadAssignment.Application.Assignments.Commands.CheckIn;
using LeadAssignment.Application.Assignments.Commands.CheckOut;
using LeadAssignment.Application.Assignments.Commands.UpdateSlaConfig;
using LeadAssignment.Application.Assignments.Queries.GetAssignmentReport;
using LeadAssignment.Application.Assignments.Queries.GetCustomerAssignmentHistory;
using LeadAssignment.Application.Common.Interfaces;
using Shared.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadAssignment.API.Controllers
{
    /// <summary>
    /// Quản lý Assignment Queue & SLA status cho tất cả các hệ đào tạo
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireCustomerCareOrAdmin")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentDbContext _context;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public AssignmentController(IAssignmentDbContext context, IMediator mediator, ICurrentUserService currentUserService)
        {
            _context = context;
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpPost("check-in")]
        public async Task<ActionResult> CheckIn()
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            var command = new CheckInCommand { ConsultantId = _currentUserService.UserId.Value };
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Đã bật trạng thái nhận khách hàng" });
        }

        [HttpPost("check-out")]
        public async Task<ActionResult> CheckOut()
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            var command = new CheckOutCommand { ConsultantId = _currentUserService.UserId.Value };
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Đã tắt trạng thái nhận khách hàng" });
        }

        [HttpPut("config/sla")]
        public async Task<ActionResult> UpdateSlaConfig([FromBody] UpdateSlaConfigCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Cập nhật cấu hình SLA thành công" });
        }

        [HttpGet("report")]
        public async Task<ActionResult<List<AssignmentReportDto>>> GetReport([FromQuery] GetAssignmentReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        [HttpGet("history/{customerId}")]
        public async Task<ActionResult<List<CustomerAssignmentHistoryDto>>> GetAssignmentHistory(Guid customerId)
        {
            var query = new GetCustomerAssignmentHistoryQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Xem tình trạng queue giao khách
        /// </summary>
        [HttpGet("queue")]
        public async Task<ActionResult> GetQueueStatus([FromQuery] TrainingSystem? trainingSystem)
        {
            var query = _context.AssignmentQueues.AsQueryable();
            if (trainingSystem.HasValue)
            {
                query = query.Where(q => q.TrainingSystem == trainingSystem.Value);
            }

            var queue = await query
                .OrderBy(q => q.TrainingSystem)
                .ThenBy(q => q.OrderIndex)
                .Join(_context.UserReplicas,
                    q => q.ConsultantId,
                    u => u.Id,
                    (q, u) => new
                    {
                        q.Id,
                        TrainingSystem = q.TrainingSystem.ToString(),
                        ConsultantName = u.FullName,
                        q.ConsultantId,
                        q.OrderIndex,
                        q.CurrentLoad,
                        q.MaxLoad,
                        q.IsActive,
                        q.LastAssignedAt,
                    })
                .ToListAsync();

            return Ok(queue);
        }

        /// <summary>
        /// Xem danh sách SLA đang active (chưa liên hệ)
        /// </summary>
        [HttpGet("sla/active")]
        public async Task<ActionResult> GetActiveSla([FromQuery] TrainingSystem? trainingSystem)
        {
            var query = _context.CustomerCareStatuses
                .Where(s => !s.IsContactMade && !s.IsReassigned);

            if (trainingSystem.HasValue)
            {
                query = query.Where(s => s.TrainingSystem == trainingSystem.Value);
            }

            var slaList = await query
                .OrderBy(s => s.Deadline)
                .Join(_context.UserReplicas,
                    s => s.AssigneeId,
                    u => u.Id,
                    (s, u) => new
                    {
                        s.Id,
                        s.CustomerName,
                        s.CustomerId,
                        TrainingSystem = s.TrainingSystem.ToString(),
                        AssigneeName = u.FullName,
                        s.AssigneeId,
                        s.AssignedAt,
                        s.Deadline,
                        RemainingMinutes = EF.Functions.DateDiffMinute(DateTime.UtcNow, s.Deadline),
                        s.IsViolated,
                    })
                .ToListAsync();

            return Ok(slaList);
        }

        /// <summary>
        /// Xem bằng chứng liên hệ của 1 khách hàng
        /// </summary>
        [HttpGet("evidence/{customerId}")]
        public async Task<ActionResult> GetContactEvidences(Guid customerId)
        {
            var evidences = await _context.ContactEvidences
                .Where(e => e.CustomerId == customerId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var consultantIds = evidences.Select(e => e.ConsultantId).Distinct().ToList();
            var consultantMap = await _context.UserReplicas
                .Where(u => consultantIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = evidences.Select(e => new
            {
                e.Id,
                ConsultantName = consultantMap.GetValueOrDefault(e.ConsultantId, "N/A"),
                e.Type,
                e.FileUrl,
                e.Description,
                e.DurationSeconds,
                e.OldStatusValue,
                e.NewStatusValue,
                e.CreatedAt,
            });

            return Ok(result);
        }
    }
}

