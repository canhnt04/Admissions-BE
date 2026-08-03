using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;

using LeadAssignment.Domain.Entities;
using LeadAssignment.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.UpdateSlaConfig
{
    public class UpdateSlaConfigCommandHandler : IRequestHandler<UpdateSlaConfigCommand, Result<bool>>
    {
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IAssignmentDbContext _context;

        public UpdateSlaConfigCommandHandler(ISystemConfigRepository systemConfigRepository, IAssignmentDbContext context)
        {
            _systemConfigRepository = systemConfigRepository;
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateSlaConfigCommand request, CancellationToken cancellationToken)
        {
            if (request.SlaDeadlineMinutes.HasValue)
            {
                var slaConfig = await _systemConfigRepository.FirstOrDefaultAsync(x => x.Id == "SlaDeadlineMinutes", cancellationToken);
                if (slaConfig == null)
                {
                    _systemConfigRepository.Add(new SystemConfig { Id = "SlaDeadlineMinutes", Value = request.SlaDeadlineMinutes.Value.ToString(), Description = "SLA Deadline in minutes" });
                }
                else
                {
                    slaConfig.Value = request.SlaDeadlineMinutes.Value.ToString();
                    _systemConfigRepository.Update(slaConfig);
                }
            }

            if (request.DefaultManagerId.HasValue)
            {
                var managerConfig = await _systemConfigRepository.FirstOrDefaultAsync(x => x.Id == "DefaultManagerId", cancellationToken);
                if (managerConfig == null)
                {
                    _systemConfigRepository.Add(new SystemConfig { Id = "DefaultManagerId", Value = request.DefaultManagerId.Value.ToString(), Description = "Default Manager ID for SLA 3-strikes escalation" });
                }
                else
                {
                    managerConfig.Value = request.DefaultManagerId.Value.ToString();
                    _systemConfigRepository.Update(managerConfig);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
