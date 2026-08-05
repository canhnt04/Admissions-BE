using System.ComponentModel;

namespace Customer.Domain.Enums
{
    public enum LeadStatus
    {
        [Description("Chưa liên hệ / Không nghe / Hẹn lại")] New = 1,
        [Description("Sẽ quan tâm")] Will = 2,
        [Description("Sai / Nhầm số")] WrongNumber = 3,
        [Description("Không liên lạc được")] NotIdentified = 4,
        [Description("Lạnh nhạt / Khó chịu")] Cold = 5,
        [Description("Quan tâm")] Warm = 6,
        [Description("Rất quan tâm")] Hot = 7,
        [Description("Đã đăng ký")] Profiled = 8,
        [Description("Đã đóng cọc")] Deposited = 9,
        [Description("Đã đóng học phí")] Paid = 10,
        [Description("Đã hủy đăng ký")] ProfileCanceled = 11,
        [Description("Đã rút hồ sơ / Hoàn phí")] Withdrawn = 12,
        [Description("Hủy cọc")] CanceledDeposit = 13,
        [Description("Khách hàng không còn quan tâm hoặc từ chối")] Lost = 14
    }
}
