using Driving.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace Driving.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Hanh dong (Create/Update/Delete)
        /// </summary>
        public Enums.Action Action { get; set; }

        // Mo ta chi tiet
        public string Detail { get; set; }

        /// <summary>
        /// ID ban ghi bi tac dong
        /// </summary>
        public Guid RecordId { get; set; }

        public string RecordDesc { get; set; }

        /// <summary>
        /// Loai entity (Customer, Course...)
        /// </summary>
        public RecordEntity RecordEntity { get; set; }

        public DateTime CreationDate { get; set; }

        // User thuc hien
        public Guid UserId { get; set; }
    }
}

