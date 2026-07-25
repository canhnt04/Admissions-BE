using System;
using System.Collections.Generic;

namespace Crm.Domain.Entities
{
    public class Team
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Loại nhóm (Admission, Marketing, CustomerCare, Elementary, Formal, Driving)
        /// </summary>
        public RoleTeam? RoleTeam { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Danh sách thành viên trong nhóm
        /// </summary>
        public virtual ICollection<User> Members { get; set; } = new List<User>();
    }
}
