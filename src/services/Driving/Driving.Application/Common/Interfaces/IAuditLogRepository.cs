using Driving.Domain.Entities;
using Shared.Common.Interfaces;

namespace Driving.Application.Common.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
}
