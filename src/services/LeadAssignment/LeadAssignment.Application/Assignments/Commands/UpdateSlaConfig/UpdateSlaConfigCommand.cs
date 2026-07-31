using MediatR;
using Shared.Common;

namespace LeadAssignment.Application.Assignments.Commands.UpdateSlaConfig
{
    public class UpdateSlaConfigCommand : IRequest<Result<bool>>
    {
        public int? SlaDeadlineMinutes { get; set; }
        public Guid? DefaultManagerId { get; set; }
    }
}
