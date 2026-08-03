using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;

using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.CheckIn
{
    public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<bool>>
    {
        private readonly IAssignmentQueueRepository _assignmentQueueRepository;
        private readonly IAssignmentDbContext _context;

        public CheckInCommandHandler(IAssignmentQueueRepository assignmentQueueRepository, IAssignmentDbContext context)
        {
            _assignmentQueueRepository = assignmentQueueRepository;
            _context = context;
        }

        public async Task<Result<bool>> Handle(CheckInCommand request, CancellationToken cancellationToken)
        {
            var queue = await _assignmentQueueRepository
                .FirstOrDefaultAsync(q => q.ConsultantId == request.ConsultantId && q.TrainingSystem == TrainingSystem.ShortTerm, cancellationToken);

            if (queue == null)
            {
                queue = new AssignmentQueue
                {
                    Id = Guid.NewGuid(),
                    ConsultantId = request.ConsultantId,
                    TrainingSystem = TrainingSystem.ShortTerm,
                    MaxLoad = 10,
                    CurrentLoad = 0,
                    IsActive = true,
                    OrderIndex = 0
                };
                _assignmentQueueRepository.Add(queue);
            }
            else
            {
                queue.IsActive = true;
                _assignmentQueueRepository.Update(queue);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
