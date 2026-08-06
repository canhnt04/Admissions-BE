using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Assignments.Commands.CheckIn;
using LeadAssignment.Application.Assignments.Commands.CheckOut;
using LeadAssignment.Application.Assignments.Commands.ManualAssign;
using LeadAssignment.Application.Assignments.Commands.CreateContactEvidence;
using LeadAssignment.Application.Assignments.Queries.GetActiveSla;
using LeadAssignment.Application.Assignments.Queries.GetAssignmentReport;
using LeadAssignment.Application.Assignments.Queries.GetCustomerCareEvidence;
using LeadAssignment.Application.Assignments.Queries.GetCustomerAssignmentHistory;
using LeadAssignment.Application.Assignments.Queries.GetQueueStatus;
using Shared.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadAssignment.API.Controllers
{
    /// <summary>
    /// Quản lý Assignment Queue &amp; SLA status cho tất cả các hệ đào tạo
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireCustomerCareOrAdmin")]
    public class AssignmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public AssignmentController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Bật trạng thái sẵn sàng nhận khách hàng (Tư vấn viên)
        /// </summary>
        [HttpPost("check-in")]
        public async Task<ActionResult> CheckIn()
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            var roleTeamClaim = _currentUserService.RoleTeam;
            TrainingSystem trainingSystem;

            switch (roleTeamClaim)
            {
                case "4":
                    trainingSystem = TrainingSystem.ShortTerm;
                    break;
                case "5":
                    trainingSystem = TrainingSystem.Formal;
                    break;
                case "6":
                    trainingSystem = TrainingSystem.Driving;
                    break;
                default:
                    return Forbid();    
            }

            var command = new CheckInCommand 
            { 
                ConsultantId = _currentUserService.UserId.Value,
                TrainingSystem = trainingSystem
            };
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = $"Nhân viên {trainingSystem} đã check-in thành công." });
        }

        /// <summary>
        /// Tắt trạng thái nhận khách hàng / Nghỉ làm (Tư vấn viên)
        /// </summary>
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

        /// <summary>
        /// Giao thủ công khách hàng cho nhân viên tư vấn (Admin)
        /// </summary>
        [HttpPost("manual-assign")]
        public async Task<ActionResult> ManualAssign([FromBody] ManualAssignCommand command)
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            command.AssignedById = _currentUserService.UserId.Value;
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Đã giao thủ công khách hàng thành công" });
        }



        /// <summary>
        /// Xem báo cáo thống kê hiệu suất chăm sóc khách hàng (Admin)
        /// </summary>
        [HttpGet("report")]
        public async Task<ActionResult<List<AssignmentReportDto>>> GetReport([FromQuery] GetAssignmentReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Xem lịch sử những ai đã chăm sóc khách hàng này (Admin)
        /// </summary>
        [HttpGet("history/{customerId}")]
        public async Task<ActionResult<List<CustomerAssignmentHistoryDto>>> GetAssignmentHistory(Guid customerId)
        {
            var query = new GetCustomerAssignmentHistoryQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Xem tình trạng queue giao khách toàn hệ thống (Admin)
        /// </summary>
        [HttpGet("queue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<QueueStatusDto>>> GetQueueStatus([FromQuery] TrainingSystem? trainingSystem)
        {
            var query = new GetQueueStatusQuery { TrainingSystem = trainingSystem };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Xem tình trạng queue của cá nhân (Tư vấn viên)
        /// </summary>
        [HttpGet("queue/me")]
        public async Task<ActionResult<List<QueueStatusDto>>> GetMyQueueStatus([FromQuery] TrainingSystem? trainingSystem)
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            var query = new GetQueueStatusQuery 
            { 
                TrainingSystem = trainingSystem,
                ConsultantId = _currentUserService.UserId.Value
            };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Xem danh sách SLA đang active (chưa liên hệ)
        /// </summary>
        [HttpGet("sla/active")]
        public async Task<ActionResult<List<ActiveSlaDto>>> GetActiveSla([FromQuery] TrainingSystem? trainingSystem)
        {
            var query = new GetActiveSlaQuery { TrainingSystem = trainingSystem };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }

        /// <summary>
        /// Ghi nhận bằng chứng liên hệ / Cập nhật trạng thái chăm sóc khách hàng (Dành cho Tư vấn viên)
        /// </summary>
        [HttpPost("evidence")]
        public async Task<ActionResult> CreateContactEvidence([FromBody] CreateContactEvidenceCommand command)
        {
            if (_currentUserService.UserId == null)
                return Unauthorized(new { message = "Không xác định được danh tính người dùng" });

            command.AssigneeId = _currentUserService.UserId.Value;
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Ghi nhận bằng chứng liên hệ thành công" });
        }

        /// <summary>
        /// Xem bằng chứng liên hệ của 1 khách hàng 
        /// </summary>
        [HttpGet("evidence/{customerId}")]
        public async Task<ActionResult<List<CustomerCareEvidenceDto>>> GetCustomerCareEvidences(Guid customerId)
        {
            var query = new GetCustomerCareEvidenceQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(result.Data);
        }
    }
}
