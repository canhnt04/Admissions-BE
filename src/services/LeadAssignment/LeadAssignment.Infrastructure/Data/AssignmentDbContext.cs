using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace LeadAssignment.Infrastructure.Data
{
    public class AssignmentDbContext : DbContext, IAssignmentDbContext
    {
        public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CustomTag> CustomTags { get; set; }
        public DbSet<ContactEvidence> ContactEvidences { get; set; }
        public DbSet<AssignmentQueue> AssignmentQueues { get; set; }
        public DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        public DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();



            modelBuilder.Entity<CustomerCareStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<CustomerAssignmentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CustomerId, e.AssignmentDate }).HasDatabaseName("IX_AssignmentHistory_Customer_Date");
            });

            modelBuilder.Entity<CustomTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecordDesc).HasMaxLength(500);
            });

            modelBuilder.Entity<ContactEvidence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => new { e.CustomerId, e.ConsultantId, e.CreatedAt }).HasDatabaseName("IX_ContactEvidence_Customer_Consultant_Date");
            });

            modelBuilder.Entity<AssignmentQueue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TrainingSystem, e.IsActive }).HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Active");
                entity.HasIndex(e => new { e.TrainingSystem, e.ConsultantId }).IsUnique().HasDatabaseName("IX_AssignmentQueue_TrainingSystem_Consultant_Unique");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.CreatedAt }).HasDatabaseName("IX_Notification_Recipient_Read_Date");
            });

            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
        }
    }
}


