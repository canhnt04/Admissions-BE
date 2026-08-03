using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;
namespace LeadAssignment.Domain.Enums { public enum AssignmentReason { [Description("Lead mới (tự động)")] NewLead = 1, [Description("Giao thủ công")] ManualAssign, [Description("Vi phạm SLA — giao lại")] SlaViolation, [Description("Cân bằng tải")] Rebalance, } }
