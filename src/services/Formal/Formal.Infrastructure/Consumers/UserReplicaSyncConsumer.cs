using Shared.Contracts.Events.Auth;
using Formal.Domain.Entities;
using Formal.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Formal.Infrastructure.Consumers
{
    /// <summary>
    /// MassTransit Consumer: xử lý UserSyncEvent.
    /// Khi Auth.API tạo mới hoặc cập nhật User, event này được publish.
    /// Consumer sẽ upsert (thêm hoặc cập nhật) bảng UserReplica trong CRM database nội bộ.
    /// </summary>
    public class UserReplicaSyncConsumer : IConsumer<UserSyncEvent>
    {
        private readonly FormalDbContext _context;
        private readonly ILogger<UserReplicaSyncConsumer> _logger;

        public UserReplicaSyncConsumer(FormalDbContext context, ILogger<UserReplicaSyncConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserSyncEvent> context)
        {
            var msg = context.Message;

            var existing = await _context.UserReplicas.FindAsync(msg.UserId);

            if (existing != null)
            {
                // Update
                existing.FullName = msg.FullName;
                existing.Mobile = msg.Mobile;
                existing.Role = (Role)msg.Role;
                existing.TeamId = msg.TeamId;
                existing.IsActive = msg.IsActive;
                existing.LastSyncedAt = DateTime.UtcNow;

                _logger.LogInformation("UserReplica updated: {UserId} - {FullName}", msg.UserId, msg.FullName);
            }
            else
            {
                // Insert
                _context.UserReplicas.Add(new UserReplica
                {
                    Id = msg.UserId,
                    FullName = msg.FullName,
                    Mobile = msg.Mobile,
                    Role = (Role)msg.Role,
                    TeamId = msg.TeamId,
                    IsActive = msg.IsActive,
                    LastSyncedAt = DateTime.UtcNow,
                });

                _logger.LogInformation("UserReplica created: {UserId} - {FullName}", msg.UserId, msg.FullName);
            }

            await _context.SaveChangesAsync();
        }
    }
}

