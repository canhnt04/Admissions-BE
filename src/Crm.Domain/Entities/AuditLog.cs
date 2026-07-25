using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Hành động (Create/Update/Delete/Assign/SlaViolation)
        /// Lưu ý: Tránh nhầm lẫn với System.Action của C#
        /// </summary>
        public Action Action { get; set; }

        // Mô tả chi tiết
        public string Detail { get; set; }

        /// <summary>
        /// ID bản ghi bị tác động (Guid của Customer, Course, v.v.)
        /// </summary>
        public Guid RecordId { get; set; }

        public string RecordDesc { get; set; }

        /// <summary>
        /// Loại entity (Customer, Course...)
        /// </summary>
        public RecordEntity RecordEntity { get; set; }

        public DateTime CreationDate { get; set; }

        // Khóa ngoại liên kết với User thực hiện thao tác
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
