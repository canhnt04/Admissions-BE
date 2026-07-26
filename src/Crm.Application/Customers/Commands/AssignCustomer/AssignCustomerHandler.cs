using Crm.Application.Common.Interfaces;
using MediatR;

namespace Crm.Application.Customers.Commands.AssignCustomer
{
    public class AssignCustomerHandler : IRequestHandler<AssignCustomerCommand, bool>
    {
        private readonly IAssignmentService _assignmentService;

        public AssignCustomerHandler(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        public async Task<bool> Handle(AssignCustomerCommand request, CancellationToken cancellationToken)
        {
            await _assignmentService.ManualAssignAsync(
                request.CustomerId,
                request.AssigneeId,
                request.AssignedById,
                request.Note,
                cancellationToken);

            return true;
        }
    }
}
