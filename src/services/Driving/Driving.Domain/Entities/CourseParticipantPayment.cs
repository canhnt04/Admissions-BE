using Driving.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driving.Domain.Entities
{
    public class CourseParticipantPayment
    {
        public Guid Id { get; set; }

        // Khóa ngoại liên kết với CourseParticipant (Học viên trong khóa học)
        public Guid CourseParticipantId { get; set; }
        public virtual CourseParticipant CourseParticipant { get; set; }

        /// <summary>
        /// Số tiền đóng lần này
        /// </summary>
        public int? PaymentAmount { get; set; }

        /// <summary>
        /// Ngày đóng
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// User ghi nhận — chỉ lưu ID
        /// </summary>
        public Guid? PaymentRecordBy { get; set; }
    }
}
