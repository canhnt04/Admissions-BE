using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Formal.API.Controllers
{
    /// <summary>
    /// Quản lý Assignment Queue & SLA status (nhánh Chính Quy)
    /// </summary>
    [ApiController]
    [Route("api/formal/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly ICrmDbContext _context;

        public AssignmentController(ICrmDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Xem tình trạng queue giao khách nhánh Chính Quy
        /// </summary>
        [HttpGet("queue")]
        public async Task<ActionResult> GetQueueStatus()
        {
            var queue = await _context.AssignmentQueues
                .Include(q => q.Consultant)
                .Where(q => q.TrainingSystem == TrainingSystem.Formal)
                .OrderBy(q => q.OrderIndex)
                .Select(q => new
                {
                    q.Id,
                    ConsultantName = q.Consultant.FullName,
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
        /// Xem danh sách SLA đang active (chưa liên hệ) của nhánh Chính Quy
        /// </summary>
        [HttpGet("sla/active")]
        public async Task<ActionResult> GetActiveSla()
        {
            var slaList = await _context.SlaTrackings
                .Include(s => s.Customer)
                .Include(s => s.Assignee)
                .Where(s => !s.IsContactMade && !s.IsReassigned &&
                            s.Customer.TrainingSystem == TrainingSystem.Formal)
                .OrderBy(s => s.Deadline)
                .Select(s => new
                {
                    s.Id,
                    CustomerName = s.Customer.Name,
                    s.CustomerId,
                    AssigneeName = s.Assignee.FullName,
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
        /// Xem lịch sử giao lead của 1 khách hàng
        /// </summary>
        [HttpGet("history/{customerId}")]
        public async Task<ActionResult> GetAssignmentHistory(Guid customerId)
        {
            var history = await _context.CustomerAssignmentHistories
                .Include(h => h.Assignee)
                .Include(h => h.AssignedBy)
                .Where(h => h.CustomerId == customerId)
                .OrderByDescending(h => h.AssignmentDate)
                .Select(h => new
                {
                    h.Id,
                    AssigneeName = h.Assignee.FullName,
                    AssignedByName = h.AssignedBy.FullName,
                    h.AssignmentDate,
                    h.Reason,
                    h.Note,
                })
                .ToListAsync();

            return Ok(history);
        }

        /// <summary>
        /// Xem bằng chứng liên hệ của 1 khách hàng
        /// </summary>
        [HttpGet("evidence/{customerId}")]
        public async Task<ActionResult> GetContactEvidences(Guid customerId)
        {
            var evidences = await _context.ContactEvidences
                .Include(e => e.Consultant)
                .Where(e => e.CustomerId == customerId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    ConsultantName = e.Consultant.FullName,
                    e.Type,
                    e.FileUrl,
                    e.Description,
                    e.DurationSeconds,
                    e.OldStatusValue,
                    e.NewStatusValue,
                    e.CreatedAt,
                })
                .ToListAsync();

            return Ok(evidences);
        }
    }
}
