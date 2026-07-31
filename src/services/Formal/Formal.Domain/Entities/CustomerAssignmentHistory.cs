using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Formal.Domain.Entities
{
    public class CustomerAssignmentHistory
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// User được giao (người nhận lead) — chỉ lưu ID
        /// </summary>
        public Guid AssigneeId { get; set; }

        /// <summary>
        /// User thực hiện gán (người quản lý/chia lead) — chỉ lưu ID
        /// </summary>
        public Guid AssignedById { get; set; }

        /// <summary>
        /// Ngày giờ giao
        /// </summary>
        public DateTime AssignmentDate { get; set; }

        /// <summary>
        /// Lý do giao lead (NewLead, ManualAssign, SlaViolation, Rebalance)
        /// </summary>
        public AssignmentReason Reason { get; set; }

        /// <summary>
        /// Ghi chú lý do giao/thu hồi
        /// </summary>
        public string? Note { get; set; }
    }
}
