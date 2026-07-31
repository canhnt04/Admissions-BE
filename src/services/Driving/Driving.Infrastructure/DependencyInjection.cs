using Driving.Application.Common.Behaviors;
using Driving.Application.Common.Interfaces;
using Driving.Infrastructure.Consumers;
using Driving.Infrastructure.Data;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Driving.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDrivingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Database: Branch DB ──
            services.AddDbContext<DrivingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CrmDatabase"),
                    b => b.MigrationsAssembly(typeof(DrivingDbContext).Assembly.FullName)));

            // Register as base class for DI resolution
            
            // Register interface
            services.AddScoped<IDrivingDbContext>(provider => provider.GetRequiredService<DrivingDbContext>());

            // ── MediatR + FluentValidation ──
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(IDrivingDbContext).Assembly));

            services.AddValidatorsFromAssembly(typeof(IDrivingDbContext).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // ── MassTransit (RabbitMQ) ──
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<DrivingDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.AddConsumer<UserReplicaSyncConsumer>();

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

        public static void EnsureDrivingDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DrivingDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating Driving DB: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}

