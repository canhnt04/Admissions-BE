using System.ComponentModel;
namespace Customer.Domain.Enums { public enum FollowStatus { [Description("Đã tư vấn hết kịch bản, xin kết bạn Zalo")] Will = 1, [Description("Đã tư vấn hết kịch bản, có quan tâm")] Warm = 2, [Description("Rất quan tâm, đã tư vấn đầy đủ thông tin, có ý định đăng ký học")] Hot = 3, [Description("Khách hàng không còn quan tâm hoặc từ chối")] Lost = 4 } }
