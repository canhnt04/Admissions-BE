using System.ComponentModel;

namespace Driving.Domain.Entities
{
    public enum Source
    {
        [Description("Tìm kiếm trên Google/Website")] Website = 1,
        [Description("Facebook")] Facebook = 2,
        [Description("Bảng quảng cáo")] Banner = 3,
        [Description("Đã từng học tại trường Tây Đô")] TayDo = 4,
        [Description("Người quen giới thiệu")] Reference = 5,
        [Description("Zalo")] Zalo = 6,
        [Description("Zalo Game mini app")] ZaloMini = 7,
        [Description("Hướng nghiệp trường")] ImportFile = 8,
        [Description("Nhập liệu")] DataEntry = 9,
        [Description("Bộ đội xuất ngũ")] Military = 10,
        [Description("Hotline")] Hotline = 11,
        [Description("KH Cá nhân")] PersonalCustomer = 12,
        [Description("Affiliate")] Affiliate = 13,
        [Description("Học viên trường lái")] LearnerDriver = 14,
        [Description("Google Ads")] GoogleAds = 15,
        [Description("TikTok")] TikTok = 16,
    }

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

    public enum EducationLevel
    {
        [Description("Chưa tốt nghiệp THCS")] UnderSecondarySchool = 1,
        [Description("Tốt nghiệp THCS")] SecondarySchool,
        [Description("Tốt nghiệp THPT")] HighSchool,
        [Description("Trung Cấp/Chứng chỉ nghề")] Intermediate,
        [Description("Cao đẳng")] College,
        [Description("Đại học")] Undergraduate,
        [Description("Sau đại học")] Graduate,
        [Description("Đào tạo lái xe")] DrivingTraining,
    }

    public enum SaleStatus
    {
        [Description("LOST")] Lost = 1,
        [Description("COLD")] Cold,
        [Description("WARM")] Warm,
        [Description("HOT")] Hot,
        [Description("CAPTURED")] Captured,
        [Description("WILL")] Will,
    }

    public enum TrainingSystem
    {
        [Description("Sơ cấp")] ShortTerm = 1,
        [Description("Chính quy")] Formal,
        [Description("Lái xe")] Driving,
        [Description("Kỹ năng chuyên môn")] TechnicalSkills = 4,
    }

    public enum EquivalentDegree
    {
        [Description("Chưa xác định thiếu Trường đã học")] Unknown = 0,
        [Description("THCS")] SecondarySchool = 1,
        [Description("THPT")] HighSchool,
    }

    public enum Enrollment
    {
        [Description("Đợt 1")] Dot1 = 1,
        [Description("Đợt 2")] Dot2 = 2,
        [Description("Đợt 3")] Dot3 = 3,
        [Description("Đợt 4")] Dot4 = 4,
        [Description("Đợt 5")] Dot5 = 5,
    }

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
        [Description("Khách hàng không còn quan tâm hoặc từ chối")] Lost = 14,
    }

    public enum FollowStatus
    {
        [Description("Đã tư vấn hết kịch bản, xin kết bạn Zalo")] Will = 1,
        [Description("Đã tư vấn hết kịch bản, có quan tâm")] Warm = 2,
        [Description("Rất quan tâm, đã tư vấn đầy đủ thông tin, có ý định đăng ký học")] Hot = 3,
        [Description("Khách hàng không còn quan tâm hoặc từ chối")] Lost = 4,
    }

    public enum CourseCategory
    {
        [Description("Sơ Cấp Ngắn Hạn")] SCNH = 1,
        [Description("Trung Cấp Chính Quy")] TCCQ = 2,
        [Description("Cao Đẳng Chính Quy")] CDCQ = 3,
        [Description("Đại Học Liên Thông")] DHLT = 4,
        [Description("Cao Đẳng 9 +")] CD9 = 5,
        [Description("Đào tạo lái xe")] DrivingTraining = 6,
        [Description("Kỹ năng chuyên môn")] TechnicalSkills = 7,
    }

    public enum Role
    {
        [Description("Quản trị viên")] Admin = 99,
        [Description("Người dùng")] User = 1,
        [Description("Thực tập sinh / Thử việc")] Intern = 2,
        [Description("Nhập liệu")] EntryClerk = 3,
        [Description("Marketing")] Engineer = 4,
    }

    public enum RoleTeam
    {
        [Description("Nhóm tuyển sinh")] Admission = 1,
        [Description("Nhóm marketing")] Marketing = 2,
        [Description("Nhóm chăm sóc khách hàng")] CustomerCare = 3,
        [Description("Nhóm sơ cấp")] Elementary = 4,
        [Description("Nhóm chính quy")] Formal = 5,
        [Description("Nhóm lái xe")] Driving = 6,
    }

    public enum Action
    {
        [Description("Thêm mới")] Insert = 1,
        [Description("Cập nhật")] Update,
        [Description("Xóa")] Delete,
        [Description("Giao lead")] Assign,
        [Description("Vi phạm SLA")] SlaViolation,
        [Description("Tự động giao lại")] AutoReassign,
    }

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
        [Description("Giao khách / Assignment")] Assignment,
        [Description("Bằng chứng liên hệ")] ContactEvidence,
        [Description("SLA Tracking")] SlaTracking,
    }

    // ─── New enums cho Auto-Assignment ───

    /// <summary>
    /// Lý do giao lead
    /// </summary>
    public enum AssignmentReason
    {
        [Description("Lead mới (tự động)")] NewLead = 1,
        [Description("Giao thủ công")] ManualAssign,
        [Description("Vi phạm SLA — giao lại")] SlaViolation,
        [Description("Cân bằng tải")] Rebalance,
    }

    /// <summary>
    /// Loại bằng chứng liên hệ
    /// </summary>
    public enum ContactEvidenceType
    {
        [Description("Ghi âm cuộc gọi tư vấn")] CallRecording = 1,
        [Description("Thay đổi trạng thái lead")] StatusChange,
        [Description("Ghi chú tư vấn")] Note,
        [Description("Cuộc hẹn trực tiếp")] Meeting,
        [Description("Tin nhắn Zalo")] ZaloMessage,
        [Description("Tin nhắn Facebook")] FacebookMessage,
    }

    /// <summary>
    /// Loại thông báo hệ thống
    /// </summary>
    public enum NotificationType
    {
        [Description("Giao lead mới")] LeadAssigned = 1,
        [Description("Cảnh báo SLA sắp hết hạn")] SlaWarning,
        [Description("Vi phạm SLA — lead bị thu hồi")] SlaViolation,
        [Description("Lead được giao lại")] LeadReassigned,
    }
}
