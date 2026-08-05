using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using MediatR;
using Shared.Common;
using System;

namespace LeadAssignment.Application.Assignments.Commands.AssignPendingLeads
{
    public class AssignPendingLeadsCommand : IRequest<Result<bool>>
    {
        public TrainingSystem? TrainingSystem { get; set; }
    }
}
