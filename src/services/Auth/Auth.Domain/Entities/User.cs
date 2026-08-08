using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }

        // CCCD NV
        public string IdentificationNumber { get; set; }

        // Mã nội bộ
        public string UserInternalId { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ProfilePicUrl { get; set; }
        public bool IsActived { get; set; }

        /// <summary>
        /// 1=User, 2=Intern, 3=EntryClerk, 4=Marketing, 99=Admin
        /// </summary>
        public Role Role { get; set; }

        // Khóa ngoại liên kết với Team
        public Guid? TeamId { get; set; }
        public virtual Team Team { get; set; }
    }
}
