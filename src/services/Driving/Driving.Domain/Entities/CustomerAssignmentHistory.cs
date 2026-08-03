using Driving.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System;

namespace Driving.Domain.Entities
{
    public class CustomerAssignmentHistory
    {
        public Guid Id { get; set; }

        // FK -> Customer
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// User duoc giao (nguoi nhan lead)
        /// </summary>
        public Guid AssigneeId { get; set; }
        /// <summary>
        /// User thuc hien gan (nguoi quan ly/chia lead)
        /// </summary>
        public Guid AssignedById { get; set; }
        /// <summary>
        /// Ngay gio giao
        /// </summary>
        public DateTime AssignmentDate { get; set; }
    }
}

