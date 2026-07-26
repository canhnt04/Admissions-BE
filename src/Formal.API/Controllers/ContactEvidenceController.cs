using Crm.Application.ContactEvidences.Commands.CreateContactEvidence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Formal.API.Controllers
{
    /// <summary>
    /// Upload bằng chứng liên hệ khách hàng (nhánh Chính Quy)
    /// </summary>
    [ApiController]
    [Route("api/formal/[controller]")]
    public class ContactEvidenceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactEvidenceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Upload bằng chứng liên hệ (ghi âm, ghi chú, thay đổi status...)
        /// Sau khi upload, SLA tracking sẽ tự động đánh dấu "đã liên hệ"
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContactEvidenceCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id }, id);
        }
    }
}
