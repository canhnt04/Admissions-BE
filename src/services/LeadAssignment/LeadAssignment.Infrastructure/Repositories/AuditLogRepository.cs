using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Shared.Common.Repositories;

namespace LeadAssignment.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog, AssignmentDbContext>, IAuditLogRepository
{
    public AuditLogRepository(AssignmentDbContext dbContext) : base(dbContext)
    {
    }
}
