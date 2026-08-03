using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Domain.Entities;
using LeadAssignment.Infrastructure.Data;
using Shared.Common.Repositories;

namespace LeadAssignment.Infrastructure.Repositories;

public class CustomerAssignmentHistoryRepository : GenericRepository<CustomerAssignmentHistory, AssignmentDbContext>, ICustomerAssignmentHistoryRepository
{
    public CustomerAssignmentHistoryRepository(AssignmentDbContext dbContext) : base(dbContext)
    {
    }
}
