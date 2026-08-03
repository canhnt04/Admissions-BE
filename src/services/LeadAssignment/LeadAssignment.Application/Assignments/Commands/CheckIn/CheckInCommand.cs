using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.CheckIn
{
    public class CheckInCommand : IRequest<Result<bool>>
    {
        public Guid ConsultantId { get; set; }
    }
}
