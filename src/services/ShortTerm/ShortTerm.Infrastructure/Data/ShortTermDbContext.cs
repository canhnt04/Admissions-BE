using ShortTerm.Application.Common.Interfaces;
using ShortTerm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace ShortTerm.Infrastructure.Data
{
    /// <summary>
    /// DbContext chung cho các nhánh CRM (Formal, ShortTerm, Driving).
    /// </summary>
    public class ShortTermDbContext : DbContext, IShortTermDbContext
    {
        public ShortTermDbContext(DbContextOptions<ShortTermDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseParticipant> CourseParticipants { get; set; }
        public DbSet<CourseParticipantPayment> CourseParticipantPayments { get; set; }
        public DbSet<CustomTag> CustomTags { get; set; }
        public DbSet<UserReplica> UserReplicas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<UserReplica>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Mobile).HasMaxLength(50);
            });

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

                entity.HasOne(e => e.AssigneeUser)
                      .WithMany()
                      .HasForeignKey(e => e.Assignee)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.TrainingSystem, e.Assignee })
                      .HasDatabaseName("IX_Customer_TrainingSystem_Assignee");
                entity.HasIndex(e => e.Mobile)
                      .HasDatabaseName("IX_Customer_Mobile");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<CourseParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CourseParticipantPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.CourseParticipant)
                      .WithMany()
                      .HasForeignKey(e => e.CourseParticipantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

