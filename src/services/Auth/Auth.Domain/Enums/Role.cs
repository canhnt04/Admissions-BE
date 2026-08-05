using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;

namespace Auth.Domain.Enums 
{ 
    public enum Role 
    { 
        [Description("Quản trị viên")] Admin = 99, 
        [Description("Người dùng")] User = 1, 
        [Description("Thực tập sinh / Thử việc")] Intern = 2, 
        [Description("Nhập liệu")] EntryClerk = 3, 
        [Description("Marketing")] Engineer = 4, 
    } 
}
