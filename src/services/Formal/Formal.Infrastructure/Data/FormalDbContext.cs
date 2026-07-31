using Formal.Application.Common.Interfaces;
using Formal.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Formal.Infrastructure.Data
{
    /// <summary>
    /// DbContext chung cho 3 nhánh CRM (Formal, ShortTerm, Driving).
    /// Mỗi nhánh sẽ kế thừa và kết nối tới DB riêng.
    /// Chứa tất cả bảng nghiệp vụ CRM + UserReplica (bản sao User đồng bộ qua RabbitMQ).
    /// </summary>
    public class FormalDbContext : DbContext, IFormalDbContext
    {
        public FormalDbContext(DbContextOptions<FormalDbContext> options) : base(options) { }

        // ─── Bản sao User (đồng bộ qua Event Driven) ───
        public DbSet<UserReplica> UserReplicas { get; set; }

        // ─── Existing DbSets ───
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        public DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ─── Auto-Assignment & SLA ───
        public DbSet<CustomTag> CustomTags { get; set; }
        public DbSet<ContactEvidence> ContactEvidences { get; set; }
        public DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        public DbSet<SlaTracking> SlaTrackings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // ════════════════════════════════════════════════════════
            // USER REPLICA (bản sao tối giản từ Auth service)
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<UserReplica>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Mobile).HasMaxLength(50);
            });

            // ════════════════════════════════════════════════════════
            // CUSTOMER
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerNumber).ValueGeneratedOnAdd();
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

                // Navigation → UserReplica (thay thế FK chéo sang AuthDb)
                entity.HasOne(e => e.AssigneeUser)
                      .WithMany()
                      .HasForeignKey(e => e.Assignee)
                      .OnDelete(DeleteBehavior.SetNull);

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
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ════════════════════════════════════════════════════════
            // CUSTOMER ASSIGNMENT HISTORY
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CustomerAssignmentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CustomerId, e.AssignmentDate })
                      .HasDatabaseName("IX_AssignmentHistory_Customer_Date");
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
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.CustomTag)
                      .WithMany()
                      .HasForeignKey(e => e.CustomTagId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ════════════════════════════════════════════════════════
            // COURSE PARTICIPANT
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CourseParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Referencee).HasMaxLength(50);

                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ════════════════════════════════════════════════════════
            // COURSE PARTICIPANT PAYMENT
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<CourseParticipantPayment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.CourseParticipant)
                      .WithMany()
                      .HasForeignKey(e => e.CourseParticipantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ════════════════════════════════════════════════════════
            // AUDIT LOG
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecordDesc).HasMaxLength(500);
            });

            // ════════════════════════════════════════════════════════
            // CONTACT EVIDENCE
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<ContactEvidence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.OldStatusValue).HasMaxLength(100);
                entity.Property(e => e.NewStatusValue).HasMaxLength(100);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.CustomerId, e.ConsultantId, e.CreatedAt })
                      .HasDatabaseName("IX_ContactEvidence_Customer_Consultant_Date");
            });

            // ════════════════════════════════════════════════════════
            // ASSIGNMENT QUEUE
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<AssignmentQueue>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.TrainingSystem, e.IsActive })
                      .HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Active");

                entity.HasIndex(e => new { e.TrainingSystem, e.ConsultantId })
                      .IsUnique()
                      .HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Consultant_Unique");
            });

            // ════════════════════════════════════════════════════════
            // SLA TRACKING
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<SlaTracking>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.IsContactMade, e.Deadline, e.IsReassigned })
                      .HasDatabaseName("IX_SlaTracking_ContactMade_Deadline_Reassigned");

                entity.HasIndex(e => new { e.CustomerId, e.AssigneeId })
                      .HasDatabaseName("IX_SlaTracking_Customer_Assignee");
            });

            // ════════════════════════════════════════════════════════
            // NOTIFICATION
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(1000);

                entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.CreatedAt })
                      .HasDatabaseName("IX_Notification_Recipient_Read_Date");
            // ════════════════════════════════════════════════════════
            // SYSTEM CONFIG
            // ════════════════════════════════════════════════════════
            });
        }
    }

    }
