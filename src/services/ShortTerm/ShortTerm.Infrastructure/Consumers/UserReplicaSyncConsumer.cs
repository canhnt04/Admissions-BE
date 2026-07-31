using ShortTerm.Application.Common.Interfaces;
using ShortTerm.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events.Auth;

namespace ShortTerm.Infrastructure.Consumers
{
    public class UserReplicaSyncConsumer : IConsumer<UserSyncEvent>
    {
        private readonly IShortTermDbContext _context;

        public UserReplicaSyncConsumer(IShortTermDbContext context)
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
                    Role = msg.Role,
                    TeamId = msg.TeamId,
                    IsActive = msg.IsActive,
                };
                _context.UserReplicas.Add(user);
            }
            else
            {
                user.FullName = msg.FullName;
                user.Email = msg.Email;
                user.Mobile = msg.Mobile;
                user.Role = msg.Role;
                user.TeamId = msg.TeamId;
                user.IsActive = msg.IsActive;
            }

            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}

