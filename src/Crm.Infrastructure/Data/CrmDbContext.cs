using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Infrastructure.Data
{
    public class CrmDbContext : DbContext, ICrmDbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

        // ─── Existing DbSets ───
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        public DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ─── New DbSets (Auto-Assignment & SLA) ───
        public DbSet<Team> Teams { get; set; }
        public DbSet<CustomTag> CustomTags { get; set; }
        public DbSet<ContactEvidence> ContactEvidences { get; set; }
        public DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        public DbSet<SlaTracking> SlaTrackings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ════════════════════════════════════════════════════════
            // CUSTOMER
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Customer>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Cấu hình cột tự tăng (Identity) cho CustomerNumber
                entity.Property(e => e.CustomerNumber)
                      .ValueGeneratedOnAdd();

                // Cấu hình giới hạn độ dài chuỗi
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(50);
                entity.Property(e => e.Mobile).HasMaxLength(50);
                entity.Property(e => e.StudentId).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.PlaceOfBirth).HasMaxLength(500);
                entity.Property(e => e.LatestSchool).HasMaxLength(500);
                entity.Property(e => e.OnlineMessageMobile).HasMaxLength(50);
                entity.Property(e => e.Ethnic).HasMaxLength(500);
                entity.Property(e => e.SchoolAddress).HasMaxLength(500);
                entity.Property(e => e.ParentMobile).HasMaxLength(50);

                // Cấu hình Navigation Property (Khóa ngoại Assignee liên kết với User)
                entity.HasOne(e => e.User)
                      .WithMany() // Giả sử một User có nhiều Customer được giao
                      .HasForeignKey(e => e.Assignee)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu xóa User, cột Assignee sẽ thành null thay vì xóa luôn Customer

                // ── Index cho phân nhánh + assignment lookup ──
                entity.HasIndex(e => new { e.TrainingSystem, e.Assignee })
                      .HasDatabaseName("IX_Customer_TrainingSystem_Assignee");

                entity.HasIndex(e => e.Mobile)
                      .HasDatabaseName("IX_Customer_Mobile");
            });

            // ════════════════════════════════════════════════════════
            // CUSTOMER CARE STATUS
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CustomerCareStatus>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi (string 500)
                entity.Property(e => e.Note).HasMaxLength(500);

                // Cấu hình Khóa ngoại 1: Liên kết với Customer
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Customer thì xóa luôn lịch sử chăm sóc

                // Cấu hình Khóa ngoại 2: Liên kết với User (Assignee)
                entity.HasOne(e => e.Assignee)
                      .WithMany()
                      .HasForeignKey(e => e.AssigneeId)
                      .OnDelete(DeleteBehavior.SetNull); // Tránh lỗi vòng lặp xóa, nếu User bị xóa thì set AssigneeId về null
            });

            // ════════════════════════════════════════════════════════
            // CUSTOMER ASSIGNMENT HISTORY
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CustomerAssignmentHistory>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi
                entity.Property(e => e.Note).HasMaxLength(500);

                // Khóa ngoại 1: Liên kết với Customer
                entity.HasOne(e => e.Customer)
                      .WithMany() // Điền tên Collection nếu class Customer có chứa danh sách này
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade); // Khách hàng bị xóa thì xóa luôn lịch sử chia lead

                // Khóa ngoại 2: Liên kết với User (Người được giao)
                entity.HasOne(e => e.Assignee)
                      .WithMany()
                      .HasForeignKey(e => e.AssigneeId)
                      .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi đa luồng xóa (Multiple cascade paths)

                // Khóa ngoại 3: Liên kết với User (Người thực hiện gán)
                entity.HasOne(e => e.AssignedBy)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedById)
                      .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để tránh lỗi đa luồng xóa

                // ── Index cho query lịch sử giao lead theo khách ──
                entity.HasIndex(e => new { e.CustomerId, e.AssignmentDate })
                      .HasDatabaseName("IX_AssignmentHistory_Customer_Date");
            });

            // ════════════════════════════════════════════════════════
            // USER
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<User>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi (string 50) theo thiết kế
                entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IdentificationNumber).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.UserInternalId).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Mobile).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.ProfilePicUrl).HasMaxLength(500).IsRequired(false);

                // Cấu hình Khóa ngoại: Liên kết với Team
                // (Giả định TeamId có thể null, nếu team bị xóa thì set user's TeamId về null)
                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Members)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ════════════════════════════════════════════════════════
            // TEAM
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            // ════════════════════════════════════════════════════════
            // CUSTOM TAG
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CustomTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // ════════════════════════════════════════════════════════
            // COURSE
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Course>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi theo thiết kế
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(50);
                // Trường Description không ghi giới hạn, EF Core sẽ tự động map thành NVARCHAR(MAX)

                // Cấu hình Khóa ngoại: Liên kết với CustomTag
                entity.HasOne(e => e.CustomTag)
                      .WithMany() // Điền tên ICollection<Course> bên trong class CustomTag nếu có
                      .HasForeignKey(e => e.CustomTagId)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu CustomTag bị xóa, set CustomTagId của Course về null
            });

            // ════════════════════════════════════════════════════════
            // COURSE PARTICIPANT
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CourseParticipant>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi (string 50)
                entity.Property(e => e.Referencee).HasMaxLength(50);

                // Cấu hình Khóa ngoại 1: Liên kết với Course
                entity.HasOne(e => e.Course)
                      .WithMany() // Nếu trong class Course có ICollection<CourseParticipant>, điền vào đây
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa khóa học thì xóa luôn danh sách học viên trong khóa đó

                // Cấu hình Khóa ngoại 2: Liên kết với Customer
                entity.HasOne(e => e.Customer)
                      .WithMany() // Tương tự, nếu Customer có ICollection<CourseParticipant>
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict); // Giữ Restrict để tránh lỗi đa luồng xóa với User/Course

                // Cấu hình Khóa ngoại 3 (Optional nhưng khuyên dùng): Liên kết với User (Người ghi nhận thanh toán)
                entity.HasOne(e => e.PaymentRecordByUser)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentRecordBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ════════════════════════════════════════════════════════
            // COURSE PARTICIPANT PAYMENT
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CourseParticipantPayment>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Cấu hình Khóa ngoại 1: Liên kết với CourseParticipant
                entity.HasOne(e => e.CourseParticipant)
                      .WithMany() // Nếu class CourseParticipant có ICollection<CourseParticipantPayment>, điền vào đây
                      .HasForeignKey(e => e.CourseParticipantId)
                      .OnDelete(DeleteBehavior.Cascade); // Nếu hủy đăng ký học (xóa CourseParticipant), xóa luôn các dòng lịch sử đóng tiền của người đó trong khóa này

                // Cấu hình Khóa ngoại 2: Liên kết với User (Người thu tiền/ghi nhận)
                entity.HasOne(e => e.PaymentRecordByUser)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentRecordBy)
                      .OnDelete(DeleteBehavior.SetNull); // Tránh lỗi đa luồng xóa, giữ lại lịch sử dòng tiền ngay cả khi User nghỉ việc bị xóa khỏi hệ thống
            });

            // ════════════════════════════════════════════════════════
            // AUDIT LOG
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<AuditLog>(entity =>
            {
                // Khóa chính
                entity.HasKey(e => e.Id);

                // Giới hạn độ dài chuỗi 
                entity.Property(e => e.RecordDesc).HasMaxLength(500);

                // Thuộc tính Detail thường lưu nội dung dài (như chuỗi JSON của dữ liệu cũ/mới) 
                // nên không cần HasMaxLength, EF Core sẽ tự map thành nvarchar(MAX)

                // Cấu hình Khóa ngoại: Liên kết với User
                entity.HasOne(e => e.User)
                      .WithMany() // Không bắt buộc phải có ICollection<AuditLog> trong class User
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // RẤT QUAN TRỌNG: Không cho phép xóa User nếu User đó đã có log, hoặc dùng NoAction để giữ nguyên tính toàn vẹn của lịch sử hệ thống
            });

            // ════════════════════════════════════════════════════════
            // CONTACT EVIDENCE (Bằng chứng liên hệ)
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<ContactEvidence>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.OldStatusValue).HasMaxLength(100);
                entity.Property(e => e.NewStatusValue).HasMaxLength(100);

                // FK → Customer
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK → User (NV tư vấn)
                entity.HasOne(e => e.Consultant)
                      .WithMany()
                      .HasForeignKey(e => e.ConsultantId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ── Index: tìm bằng chứng theo KH + NV nhanh ──
                entity.HasIndex(e => new { e.CustomerId, e.ConsultantId, e.CreatedAt })
                      .HasDatabaseName("IX_ContactEvidence_Customer_Consultant_Date");
            });

            // ════════════════════════════════════════════════════════
            // ASSIGNMENT QUEUE (Queue giao khách Round-Robin)
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<AssignmentQueue>(entity =>
            {
                entity.HasKey(e => e.Id);

                // FK → User (NV tư vấn)
                entity.HasOne(e => e.Consultant)
                      .WithMany()
                      .HasForeignKey(e => e.ConsultantId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ── Index: lookup queue theo nhánh + active status ──
                entity.HasIndex(e => new { e.TrainingSystem, e.IsActive })
                      .HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Active");

                // ── Unique: mỗi NV chỉ có 1 slot trong queue của 1 nhánh ──
                entity.HasIndex(e => new { e.TrainingSystem, e.ConsultantId })
                      .IsUnique()
                      .HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Consultant_Unique");
            });

            // ════════════════════════════════════════════════════════
            // SLA TRACKING (Theo dõi SLA 30 phút)
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<SlaTracking>(entity =>
            {
                entity.HasKey(e => e.Id);

                // FK → Customer
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK → User (NV đang giữ lead)
                entity.HasOne(e => e.Assignee)
                      .WithMany()
                      .HasForeignKey(e => e.AssigneeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK → User (NV được giao lại — nullable)
                entity.HasOne(e => e.ReassignedTo)
                      .WithMany()
                      .HasForeignKey(e => e.ReassignedToId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ── Index QUAN TRỌNG: Background service check SLA mỗi phút ──
                // Query: WHERE IsContactMade = false AND Deadline < NOW AND IsReassigned = false
                entity.HasIndex(e => new { e.IsContactMade, e.Deadline, e.IsReassigned })
                      .HasDatabaseName("IX_SlaTracking_ContactMade_Deadline_Reassigned");

                // ── Index: tìm SLA tracking theo KH + NV ──
                entity.HasIndex(e => new { e.CustomerId, e.AssigneeId })
                      .HasDatabaseName("IX_SlaTracking_Customer_Assignee");
            });

            // ════════════════════════════════════════════════════════
            // NOTIFICATION (Thông báo in-app)
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(1000);

                // FK → User (Người nhận)
                entity.HasOne(e => e.Recipient)
                      .WithMany()
                      .HasForeignKey(e => e.RecipientId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ── Index: lấy notification chưa đọc của 1 user nhanh ──
                entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.CreatedAt })
                      .HasDatabaseName("IX_Notification_Recipient_Read_Date");
            });
        }
    }
}
