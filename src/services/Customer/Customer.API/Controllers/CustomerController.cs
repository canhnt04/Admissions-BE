using Customer.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Shared.Common.Controllers;
using Shared.Common;

namespace Customer.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : BaseApiController
    {
        private readonly CustomerSeeder _seeder;

        public CustomerController(CustomerSeeder seeder)
        {
            _seeder = seeder;
        }

        [HttpPost("seed-customers")]
        public async Task<IActionResult> SeedCustomers([FromQuery] int count = 100)
        {
            await _seeder.SeedAsync(count);
            return HandleResult(Result.Success());
        }
    }
}
