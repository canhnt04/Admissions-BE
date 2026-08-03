using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;

using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.CheckOut
{
    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<bool>>
    {
        private readonly IAssignmentQueueRepository _assignmentQueueRepository;
        private readonly IAssignmentDbContext _context;

        public CheckOutCommandHandler(IAssignmentQueueRepository assignmentQueueRepository, IAssignmentDbContext context)
        {
            _assignmentQueueRepository = assignmentQueueRepository;
            _context = context;
        }

        public async Task<Result<bool>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            var queue = await _assignmentQueueRepository
                .FirstOrDefaultAsync(q => q.ConsultantId == request.ConsultantId && q.TrainingSystem == TrainingSystem.ShortTerm, cancellationToken);

            if (queue != null)
            {
                queue.IsActive = false;
                _assignmentQueueRepository.Update(queue);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<bool>.Success(true);
        }
    }
}
