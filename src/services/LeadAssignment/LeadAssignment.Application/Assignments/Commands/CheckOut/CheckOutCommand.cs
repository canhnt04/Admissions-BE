using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.CheckOut
{
    public class CheckOutCommand : IRequest<Result<bool>>
    {
        public Guid ConsultantId { get; set; }
    }
}
