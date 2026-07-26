using Crm.Application.ContactEvidences.Commands.CreateContactEvidence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ShortTerm.API.Controllers
{
    [ApiController]
    [Route("api/shortterm/[controller]")]
    public class ContactEvidenceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactEvidenceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateContactEvidenceCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id }, id);
        }
    }
}
