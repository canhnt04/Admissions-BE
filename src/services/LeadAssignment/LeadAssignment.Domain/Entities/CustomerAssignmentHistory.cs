using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LeadAssignment.Domain.Entities
{
    public class CustomerAssignmentHistory
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid AssigneeId { get; set; }
        public Guid AssignedById { get; set; }
        public DateTime AssignmentDate { get; set; }
    }
}

