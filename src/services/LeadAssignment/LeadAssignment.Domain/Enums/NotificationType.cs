using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;
namespace LeadAssignment.Domain.Enums { public enum NotificationType { [Description("Giao lead mới")] LeadAssigned = 1, [Description("Cảnh báo SLA sắp hết hạn")] SlaWarning, [Description("Vi phạm SLA — lead bị thu hồi")] SlaViolation, [Description("Lead được giao lại")] LeadReassigned, } }
