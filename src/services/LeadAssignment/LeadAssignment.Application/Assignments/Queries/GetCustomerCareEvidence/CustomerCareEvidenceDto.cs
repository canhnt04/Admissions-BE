using System;
using Customer.Domain.Enums;
using LeadAssignment.Domain.Enums;
using Shared.Contracts.Enums;

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerCareEvidence
{
    public class CustomerCareEvidenceDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public TrainingSystem? TrainingSystem { get; set; }
        public Guid? AssigneeId { get; set; }
        public LeadStatus? Status { get; set; }
        public FollowStatus? FollowStatus { get; set; }
        public DateTime? StatusDate { get; set; }
        public DateTime? ReportDate { get; set; }
        public string? Note { get; set; }
    }
}
