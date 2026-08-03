using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Shared.Common.Repositories;

namespace LeadAssignment.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification, AssignmentDbContext>, INotificationRepository
{
    public NotificationRepository(AssignmentDbContext dbContext) : base(dbContext)
    {
    }
}
