using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;
namespace LeadAssignment.Domain.Enums 
{ 
    public enum Action 
    { 
        [Description("Thêm mới")] Create = 1, 
        [Description("Cập nhật")] Update = 2, 
        [Description("Xóa")] Delete = 3, 
        [Description("Assign")] Assign = 4, 
        [Description("Vi phạm SLA")] SlaViolation = 5
    } 
}
