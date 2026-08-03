using Driving.Application.Common.Interfaces;
using Driving.Domain.Entities;
using Driving.Infrastructure.Data;
using Shared.Common.Repositories;

namespace Driving.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog, DrivingDbContext>, IAuditLogRepository
{
    public AuditLogRepository(DrivingDbContext dbContext) : base(dbContext)
    {
    }
}
