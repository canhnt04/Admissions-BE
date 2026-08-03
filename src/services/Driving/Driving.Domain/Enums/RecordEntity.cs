using System.ComponentModel;
namespace Driving.Domain.Enums
{
    public enum RecordEntity
    {
        [Description("Khách hàng")] Customer,
        [Description("Khóa học")] Course,
        [Description("Hồ sơ khách hàng")] ClientDocument,
        [Description("Đăng ký khóa học")] CourseParticipant,
        [Description("Ghi chú")] CustomerNote,
        [Description("Nhóm tuyển sinh")] AdmissionResultGroup,
        [Description("Kết quả tuyển sinh")] AdmissionResult,
        [Description("System Tag")] CustomTag,
    }
}
