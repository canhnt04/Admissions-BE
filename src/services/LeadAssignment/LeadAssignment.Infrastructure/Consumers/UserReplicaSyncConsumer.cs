using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events.Auth;

namespace LeadAssignment.Infrastructure.Consumers
{
    public class UserReplicaSyncConsumer : IConsumer<UserSyncEvent>
    {
        private readonly IAssignmentDbContext _context;

        public UserReplicaSyncConsumer(IAssignmentDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<UserSyncEvent> context)
        {
            var msg = context.Message;
            var user = await _context.UserReplicas.FirstOrDefaultAsync(u => u.Id == msg.UserId);

            if (user == null)
            {
                user = new UserReplica
                {
                    Id = msg.UserId,
                    FullName = msg.FullName,
                    Email = msg.Email,
                    Mobile = msg.Mobile,
                    Role = (Role)msg.Role,
                    TeamId = msg.TeamId,
                    IsActive = msg.IsActive,
                    LastSyncedAt = DateTime.UtcNow
                };
                _context.UserReplicas.Add(user);
            }
            else
            {
                user.FullName = msg.FullName;
                user.Email = msg.Email;
                user.Mobile = msg.Mobile;
                user.Role = (Role)msg.Role;
                user.TeamId = msg.TeamId;
                user.IsActive = msg.IsActive;
                user.LastSyncedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
