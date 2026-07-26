using Crm.Application.Customers.Commands.AssignCustomer;
using Crm.Application.Customers.Commands.CreateCustomer;
using Crm.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Formal.API.Controllers
{
    /// <summary>
    /// Quản lý khách hàng nhánh Chính Quy (TrainingSystem = Formal)
    /// </summary>
    [ApiController]
    [Route("api/formal/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tạo khách hàng mới — hệ thống sẽ tự động giao lead (Round-Robin)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCustomerCommand command)
        {
            // Đảm bảo KH thuộc nhánh Chính Quy
            command.TrainingSystem = TrainingSystem.Formal;
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id }, id);
        }

        /// <summary>
        /// Giao lead thủ công cho NV cụ thể (Admin/Manager)
        /// </summary>
        [HttpPost("assign")]
        public async Task<ActionResult> Assign([FromBody] AssignCustomerCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Giao lead thành công" });
        }
    }
}
