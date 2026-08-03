using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Application.Features.Customers.Commands.CreateCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Formal.API.Controllers
{
    [ApiController]
    [Route("api/formal/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Nhập liệu Khách hàng mới (Dành cho Marketing/Nhập liệu)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Error != Shared.Common.Error.None) return BadRequest(result.Error);
            return Ok(new { message = "Thêm khách hàng thành công và đã tự động giao cho Tư vấn viên", customerId = result.Data });
        }
    }
}
