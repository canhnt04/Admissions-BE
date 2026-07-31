using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driving.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Mã số khách hàng tự tăng
        /// </summary>
        public int CustomerNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        // Mã sinh viên
        public string StudentId { get; set; }

        // Nguồn (Web, FB, Zalo...)
        public Source? Source { get; set; }

        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateTime { get; set; }

        // User tạo
        public Guid CreatedBy { get; set; }
        // User được giao (lead) — chỉ lưu ID, không FK chéo sang AuthDb
        public Guid? Assignee { get; set; }

        // Navigation → UserReplica (bản sao nội bộ, đồng bộ qua RabbitMQ)
        public virtual UserReplica? AssigneeUser { get; set; }

        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }

        // Trạng thái KH
        public CustomerStatus? Status { get; set; }

        // Bậc học
        public EducationLevel? EducationLevel { get; set; }

        // Trạng thái bán
        public SaleStatus? SaleStatus { get; set; }

        /// <summary>
        /// PHÂN NHÁNH: 1=SơCấp, 2=ChínhQuy, 3=LáiXe
        /// </summary>
        public TrainingSystem? TrainingSystem { get; set; }

        // Tương đương bằng
        public EquivalentDegree? EquivalentDegree { get; set; }

        // Nơi sinh
        public string PlaceOfBirth { get; set; }

        // Trường mới tốt nghiệp
        public string LatestSchool { get; set; }
        public string OnlineMessageMobile { get; set; }

        // Dân tộc
        public string Ethnic { get; set; }

        // Ngày nộp hồ sơ
        public DateTime? SubmissionDate { get; set; }

        public string SchoolAddress { get; set; }

        // User ID từ OA Zalo
        public string UserIdByOa { get; set; }
        public string ParentMobile { get; set; }
        public string CCCD { get; set; }

        // Ngày cấp CCCD
        public DateTime? CCCDIssueDate { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }

        /// <summary>
        /// Trạng thái lead cuối (Hot/Warm/Cold)
        /// </summary>
        public LeadStatus? FinalStatus { get; set; }

        // Năm tốt nghiệp
        public int? GraduationYear { get; set; }

        // Trạng thái nhập học
        public Enrollment? Enrollment { get; set; }
    }
}
