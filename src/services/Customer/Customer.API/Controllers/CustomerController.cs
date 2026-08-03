using Customer.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Customer.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
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
            return Ok(new { Message = $"Seeded {count} customers and published events." });
        }
    }
}
