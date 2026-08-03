using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Shared.Common.Repositories;

namespace LeadAssignment.Infrastructure.Repositories;

public class SystemConfigRepository : GenericRepository<SystemConfig, AssignmentDbContext>, ISystemConfigRepository
{
    public SystemConfigRepository(AssignmentDbContext dbContext) : base(dbContext)
    {
    }
}
