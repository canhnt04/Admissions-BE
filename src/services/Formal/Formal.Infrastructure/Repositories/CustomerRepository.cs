using Formal.Application.Common.Interfaces;
using Formal.Infrastructure.Data;
using Shared.Common.Repositories;

namespace Formal.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<Formal.Domain.Entities.Customer, FormalDbContext>, ICustomerRepository
{
    public CustomerRepository(FormalDbContext dbContext) : base(dbContext)
    {
    }
}
