using ShortTerm.Application.Common.Interfaces;
using ShortTerm.Infrastructure.Data;
using Shared.Common.Repositories;

namespace ShortTerm.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<ShortTerm.Domain.Entities.Customer, ShortTermDbContext>, ICustomerRepository
{
    public CustomerRepository(ShortTermDbContext dbContext) : base(dbContext)
    {
    }
}
