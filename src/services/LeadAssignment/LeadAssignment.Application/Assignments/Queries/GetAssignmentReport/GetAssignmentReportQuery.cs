using MediatR;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetAssignmentReport
{
    public class GetAssignmentReportQuery : IRequest<Result<List<AssignmentReportDto>>>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AssignmentReportDto
    {
        public Guid ConsultantId { get; set; }
        public string ConsultantName { get; set; }
        public int TotalAssigned { get; set; }
        public int SlaFulfilled { get; set; }
        public int SlaViolated { get; set; }
        public int Pending { get; set; }
    }
}
