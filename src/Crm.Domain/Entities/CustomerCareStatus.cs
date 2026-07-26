using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Domain.Entities
{
    public class CustomerCareStatus
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // Khóa ngoại liên kết với User (người được giao)
        public Guid? AssigneeId { get; set; }
        public virtual User Assignee { get; set; }

        /// <summary>
        /// Trạng thái lead (Hot/Warm/Cold/Converted)
        /// </summary>
        public LeadStatus? Status { get; set; }

        /// <summary>
        /// Trạng thái follow (Contacted/Callback/...)
        /// </summary>
        public FollowStatus? FollowStatus { get; set; }

        public DateTime? StatusDate { get; set; }
        public DateTime? ReportDate { get; set; }

        public string Note { get; set; }
    }
}
