using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Domain.Entities
{
    public class CourseParticipant
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với Course
        public Guid CourseId { get; set; }
        public virtual Course Course { get; set; }

        // Khóa ngoại liên kết với Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// User ghi nhận (Khóa ngoại ngầm định liên kết tới bảng User)
        /// </summary>
        public Guid? PaymentRecordBy { get; set; }
        public virtual User PaymentRecordByUser { get; set; }

        public string Referencee { get; set; }
        public int? ReferenceePayment { get; set; }

        public CustomerStatus? Status { get; set; }
        public SaleStatus? SaleStatus { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? RemainingAmount { get; set; }
    }
}
