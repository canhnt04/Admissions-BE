using System;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Application.Common.Interfaces
{
    public interface IUserEventPublisher
    {
        Task PublishUserSyncAsync(Guid userId, string fullName, string email, string? mobile, int role, Guid? teamId, bool isActive, CancellationToken cancellationToken = default);
    }
}
