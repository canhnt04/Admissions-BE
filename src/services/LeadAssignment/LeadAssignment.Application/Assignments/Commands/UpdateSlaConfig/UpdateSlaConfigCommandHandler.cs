
using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.UpdateSlaConfig
{
    public class UpdateSlaConfigCommandHandler : IRequestHandler<UpdateSlaConfigCommand, Result<bool>>
    {
        private readonly IAssignmentDbContext _context;

        public UpdateSlaConfigCommandHandler(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateSlaConfigCommand request, CancellationToken cancellationToken)
        {
            if (request.SlaDeadlineMinutes.HasValue)
            {
                var slaConfig = await _context.SystemConfigs.FindAsync(new object[] { "SlaDeadlineMinutes" }, cancellationToken);
                if (slaConfig == null)
                {
                    _context.SystemConfigs.Add(new SystemConfig { Id = "SlaDeadlineMinutes", Value = request.SlaDeadlineMinutes.Value.ToString(), Description = "SLA Deadline in minutes" });
                }
                else
                {
                    slaConfig.Value = request.SlaDeadlineMinutes.Value.ToString();
                }
            }

            if (request.DefaultManagerId.HasValue)
            {
                var managerConfig = await _context.SystemConfigs.FindAsync(new object[] { "DefaultManagerId" }, cancellationToken);
                if (managerConfig == null)
                {
                    _context.SystemConfigs.Add(new SystemConfig { Id = "DefaultManagerId", Value = request.DefaultManagerId.Value.ToString(), Description = "Default Manager ID for SLA 3-strikes escalation" });
                }
                else
                {
                    managerConfig.Value = request.DefaultManagerId.Value.ToString();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
