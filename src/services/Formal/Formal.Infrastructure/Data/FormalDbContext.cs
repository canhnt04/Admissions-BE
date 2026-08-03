using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Application.Common.Interfaces;
using Formal.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Formal.Infrastructure.Data
{
    /// <summary>
    /// DbContext cho nhánh Formal.
    /// Chứa các bảng nghiệp vụ CRM + UserReplica (bản sao User đồng bộ qua RabbitMQ).
    /// </summary>
    public class FormalDbContext : DbContext, IFormalDbContext
    {
        public FormalDbContext(DbContextOptions<FormalDbContext> options) : base(options) { }

        // ─── Bản sao User (đồng bộ qua Event Driven) ───

        // ─── CRM Entities ───
        public DbSet<Formal.Domain.Entities.Customer> Customers { get; set; }
        public DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        public DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CustomTag> CustomTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();



            // ════════════════════════════════════════════════════════
            // CUSTOMER
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<Formal.Domain.Entities.Customer>(entity =>
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

                // Navigation → UserReplica


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

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);


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
        }
    }
}

