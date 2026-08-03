using Formal.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Formal.Application.Common.Behaviors;
using Formal.Application.Common.Interfaces;
using Formal.Infrastructure.Data;
using Formal.Infrastructure.Repositories;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Formal.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFormalInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Database: Branch DB ──
            services.AddDbContext<FormalDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CrmDatabase"),
                    b => b.MigrationsAssembly(typeof(FormalDbContext).Assembly.FullName)));

            // Register interface
            services.AddScoped<IFormalDbContext>(provider => provider.GetRequiredService<FormalDbContext>());

            // ── Repositories ──
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            // ── MediatR + FluentValidation ──
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(IFormalDbContext).Assembly));

            services.AddValidatorsFromAssembly(typeof(IFormalDbContext).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // ── MassTransit (RabbitMQ) ──
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<FormalDbContext>(o =>
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

        public static void EnsureFormalDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FormalDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating Formal DB: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}
