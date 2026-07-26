using Crm.Application.Customers.Commands.AssignCustomer;
using Crm.Application.Customers.Commands.CreateCustomer;
using Crm.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ShortTerm.API.Controllers
{
    /// <summary>
    /// Quản lý khách hàng nhánh Ngắn Hạn (TrainingSystem = ShortTerm)
    /// </summary>
    [ApiController]
    [Route("api/shortterm/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCustomerCommand command)
        {
            command.TrainingSystem = TrainingSystem.ShortTerm;
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id }, id);
        }

        [HttpPost("assign")]
        public async Task<ActionResult> Assign([FromBody] AssignCustomerCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Giao lead thành công" });
        }
    }
}
