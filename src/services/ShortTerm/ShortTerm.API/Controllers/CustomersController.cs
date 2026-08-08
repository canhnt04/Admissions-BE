using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using ShortTerm.Application.Features.Customers.Commands.CreateCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Controllers;

namespace ShortTerm.API.Controllers
{
    [ApiController]
    [Route("api/shortterm/[controller]")]
    public class CustomersController : BaseApiController
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
            return HandleResult(result);
        }
    }
}
