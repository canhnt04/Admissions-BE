using MediatR;
using Shared.Common;
using System.Collections.Generic;
using System;

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerAssignmentHistory
{
    public class GetCustomerAssignmentHistoryQuery : IRequest<Result<List<CustomerAssignmentHistoryDto>>>
    {
        public Guid CustomerId { get; set; }
    }

    public class CustomerAssignmentHistoryDto
    {
        public Guid Id { get; set; }
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public Guid AssignedById { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
    }
}
