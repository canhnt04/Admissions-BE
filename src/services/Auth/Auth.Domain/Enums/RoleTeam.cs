using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;
namespace Auth.Domain.Enums { public enum RoleTeam { [Description("Nhóm tuyển sinh")] Admission = 1, [Description("Nhóm marketing")] Marketing = 2, [Description("Nhóm chăm sóc khách hàng")] CustomerCare = 3, [Description("Nhóm sơ cấp")] Elementary = 4, [Description("Nhóm chính quy")] Formal = 5, [Description("Nhóm lái xe")] Driving = 6, } }
