using Driving.Application.Common.Interfaces;
using Driving.Infrastructure.Data;
using Shared.Common.Repositories;

namespace Driving.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<Driving.Domain.Entities.Customer, DrivingDbContext>, ICustomerRepository
{
    public CustomerRepository(DrivingDbContext dbContext) : base(dbContext)
    {
    }
}
