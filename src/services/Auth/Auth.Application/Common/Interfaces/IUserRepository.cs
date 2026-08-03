using Auth.Domain.Entities;
using Shared.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Application.Common.Interfaces
{
    /// <summary>
    /// Repository interface cho User entity — Auth service.
    /// Kế thừa IRepository<User> và bổ sung các query đặc thù nghiệp vụ.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<bool> IsUserNameTakenAsync(string userName, CancellationToken cancellationToken = default);
    }
}
