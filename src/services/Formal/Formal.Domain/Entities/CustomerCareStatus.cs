using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace Formal.Domain.Entities
{
    public class CustomerCareStatus
    {
        public Guid Id { get; set; }

        // FK -> Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // NV duoc giao cham soc
        public Guid? AssigneeId { get; set; }
        /// <summary>
        /// Trang thai lead (Hot/Warm/Cold/Converted)
        /// </summary>
        public LeadStatus? Status { get; set; }

        /// <summary>
        /// Trang thai follow (Contacted/Callback/...)
        /// </summary>
        public FollowStatus? FollowStatus { get; set; }

        public DateTime? StatusDate { get; set; }
        public DateTime? ReportDate { get; set; }

        public string Note { get; set; }
    }
}

