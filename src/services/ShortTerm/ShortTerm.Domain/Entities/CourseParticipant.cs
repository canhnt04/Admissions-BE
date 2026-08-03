using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortTerm.Domain.Entities
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
        /// User ghi nhận — chỉ lưu ID
        /// </summary>
        public Guid? PaymentRecordBy { get; set; }

        public string Referencee { get; set; }
        public int? ReferenceePayment { get; set; }

        public CustomerStatus? Status { get; set; }
        public SaleStatus? SaleStatus { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? RemainingAmount { get; set; }
    }
}
