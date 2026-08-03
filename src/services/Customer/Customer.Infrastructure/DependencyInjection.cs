using Customer.Infrastructure.Data;
using Customer.Infrastructure.Seed;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Customer.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CustomerDatabase") 
                                   ?? configuration.GetConnectionString("DefaultConnection") 
                                   ?? "Server=sqlserver;Database=CustomerDb;User Id=sa;Password=Your_Strong_Passw0rd!;TrustServerCertificate=True;";

            services.AddDbContext<CustomerDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(CustomerDbContext).Assembly.FullName)));

            services.AddScoped<CustomerSeeder>();

            // MassTransit
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<CustomerDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

        public static void EnsureCustomerDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating Customer DB: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}
