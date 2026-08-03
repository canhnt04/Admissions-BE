using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User, AuthDbContext>, IUserRepository
    {
        public UserRepository(AuthDbContext dbContext) : base(dbContext) { }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
            => await _dbSet
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        public async Task<bool> IsUserNameTakenAsync(string userName, CancellationToken cancellationToken = default)
            => await _dbSet.AnyAsync(u => u.UserName == userName, cancellationToken);
    }
}
