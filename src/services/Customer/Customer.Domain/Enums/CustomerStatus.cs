using System.ComponentModel;
namespace Customer.Domain.Enums 
{
    public enum CustomerStatus 
    { 
        [Description("Quan tâm")] Interest = 1,
        [Description("Đã đăng ký xét tuyển")] Profile = 2,
        [Description("Đã đóng cọc")] Registered = 3,
        [Description("Đã đóng học phí")] Paid = 4,
        [Description("Hủy học phí")] Withdraw = 5,
        [Description("Chờ xử lý")] AwaitingProcess = 6,
        [Description("Đã hủy đăng ký")] Canceled = 7,
        [Description("Đã hủy cọc")] CanceledDeposit = 8, 
    } 
}
