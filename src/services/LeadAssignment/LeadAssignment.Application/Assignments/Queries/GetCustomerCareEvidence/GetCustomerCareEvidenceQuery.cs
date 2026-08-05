using System;
using MediatR;
using Shared.Common;
using System.Collections.Generic;

namespace LeadAssignment.Application.Assignments.Queries.GetCustomerCareEvidence
{
    public class GetCustomerCareEvidenceQuery : IRequest<Result<List<CustomerCareEvidenceDto>>>
    {
        public Guid CustomerId { get; set; }
    }
}
