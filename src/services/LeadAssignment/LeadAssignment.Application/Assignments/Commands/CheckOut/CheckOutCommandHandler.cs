
using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.CheckOut
{
    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<bool>>
    {
        private readonly IAssignmentDbContext _context;

        public CheckOutCommandHandler(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            var queue = await _context.AssignmentQueues
                .FirstOrDefaultAsync(q => q.ConsultantId == request.ConsultantId && q.TrainingSystem == TrainingSystem.ShortTerm, cancellationToken);
            
            if (queue != null)
            {
                queue.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<bool>.Success(true);
        }
    }
}
