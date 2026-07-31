using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Auth.Application.Common.Interfaces;
using MassTransit;

namespace Auth.Infrastructure.Data
{
    /// <summary>
    /// DbContext riêng cho Auth service — chỉ chứa User & Team.
    /// Kết nối tới AuthDb.
    /// </summary>
    public class AuthDbContext : DbContext, IAuthDbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            // ════════════════════════════════════════════════════════
            // USER
            // ════════════════════════════════════════════════════════
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserName).HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasMaxLength(500);
                entity.Property(e => e.FullName).HasMaxLength(50);
                entity.Property(e => e.IdentificationNumber).HasMaxLength(50);
                entity.Property(e => e.UserInternalId).HasMaxLength(50);
                entity.Property(e => e.Mobile).HasMaxLength(50);
                entity.Property(e => e.ProfilePicUrl).HasMaxLength(50);

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
        }
    }
}
