using Bogus;
using Shared.Contracts.Events.Customer;
using Customer.Domain.Entities;
using Customer.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Customer.Infrastructure.Seed
{
    public class CustomerSeeder
    {
        private readonly CustomerDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CustomerSeeder> _logger;

        public CustomerSeeder(CustomerDbContext context, IPublishEndpoint publishEndpoint, ILogger<CustomerSeeder> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task SeedAsync(int count = 100)
        {
            if (await _context.Customers.AnyAsync())
            {
                _logger.LogInformation("Database đã có dữ liệu Customer, bỏ qua việc seed.");
                return;
            }

            _logger.LogInformation("Bắt đầu tạo {Count} Customers...", count);

            var faker = new Faker<Domain.Entities.Customer>("vi")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Mobile, f => f.Phone.PhoneNumber("09########"))
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Gender, f => f.PickRandom("Nam", "Nữ"))
                .RuleFor(c => c.Address, f => f.Address.StreetAddress())
                .RuleFor(c => c.BirthDate, f => f.Date.Past(25, DateTime.Now.AddYears(-15)))
                .RuleFor(c => c.Source, f => f.PickRandom<Domain.Enums.Source>())
                // Không set TrainingSystem ở giai đoạn tư vấn ban đầu
                .RuleFor(c => c.CreationDate, f => f.Date.Recent(30))
                .RuleFor(c => c.CreatedBy, f => Guid.NewGuid());

            var customers = faker.Generate(count);

            await _context.Customers.AddRangeAsync(customers);
            
            _logger.LogInformation("Gửi {Count} sự kiện CustomerCreatedEvent...", count);
            foreach (var c in customers)
            {
                var evt = new CustomerCreatedEvent(
                    c.Id,
                    c.Name,
                    c.Mobile,
                    c.TrainingSystem
                );
                await _publishEndpoint.Publish(evt);
            }

            // Theo như yêu cầu: Lưu DB + Gửi Event
            _logger.LogInformation("Lưu vào DB...");
            await _context.SaveChangesAsync();

            _logger.LogInformation("Hoàn tất Seed Data.");
        }
    }
}
