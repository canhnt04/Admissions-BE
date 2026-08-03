using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Auth.Application.Common.Interfaces;
using MassTransit;
using Shared.Contracts.Events.Auth;

namespace Auth.Infrastructure.Messaging.Publishers
{
    public class UserEventPublisher : IUserEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public UserEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishUserSyncAsync(Guid userId, string fullName, string email, string? mobile, int role, Guid? teamId, bool isActive, CancellationToken cancellationToken = default)
        {
            await _publishEndpoint.Publish(new UserSyncEvent
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                Mobile = mobile,
                Role = role,
                TeamId = teamId,
                IsActive = isActive
            }, cancellationToken);
        }
    }
}
