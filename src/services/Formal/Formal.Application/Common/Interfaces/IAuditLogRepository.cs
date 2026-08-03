using Formal.Domain.Entities;
using Shared.Common.Interfaces;

namespace Formal.Application.Common.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
}
