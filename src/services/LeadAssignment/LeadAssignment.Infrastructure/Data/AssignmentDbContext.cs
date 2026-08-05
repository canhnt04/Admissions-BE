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
        public DbSet<CustomerCareStatus> CustomerCareStatuses { get; set; }
        public DbSet<CustomerAssignmentHistory> CustomerAssignmentHistories { get; set; }
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

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RecordDesc).HasMaxLength(500);
            });


        }
    }
}


