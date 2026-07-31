using ShortTerm.Application.Common.Behaviors;
using ShortTerm.Application.Common.Interfaces;
using ShortTerm.Infrastructure.Consumers;
using ShortTerm.Infrastructure.Data;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShortTerm.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddShortTermInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Database: Branch DB ──
            services.AddDbContext<ShortTermDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CrmDatabase"),
                    b => b.MigrationsAssembly(typeof(ShortTermDbContext).Assembly.FullName)));

            // Register as base class for DI resolution
            
            // Register interface
            services.AddScoped<IShortTermDbContext>(provider => provider.GetRequiredService<ShortTermDbContext>());

            // ── MediatR + FluentValidation ──
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(IShortTermDbContext).Assembly));

            services.AddValidatorsFromAssembly(typeof(IShortTermDbContext).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // ── MassTransit (RabbitMQ) ──
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<ShortTermDbContext>(o =>
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

        public static void EnsureShortTermDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShortTermDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating ShortTerm DB: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}

