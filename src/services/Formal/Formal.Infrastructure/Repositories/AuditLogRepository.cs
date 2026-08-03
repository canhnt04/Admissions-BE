using Formal.Application.Common.Interfaces;
using Formal.Domain.Entities;
using Formal.Infrastructure.Data;
using Shared.Common.Repositories;

namespace Formal.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog, FormalDbContext>, IAuditLogRepository
{
    public AuditLogRepository(FormalDbContext dbContext) : base(dbContext)
    {
    }
}
