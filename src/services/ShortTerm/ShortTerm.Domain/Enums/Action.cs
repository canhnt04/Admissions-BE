using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using System.ComponentModel;
namespace ShortTerm.Domain.Enums { public enum Action { [Description("Thêm mới")] Insert = 1, [Description("Cập nhật")] Update, [Description("Xóa")] Delete, } }
