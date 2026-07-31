using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Infrastructure.Consumers;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeadAssignment.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? configuration.GetConnectionString("LeadAssignmentDatabase") 
                ?? configuration.GetConnectionString("AuthDatabase");

            services.AddDbContext<AssignmentDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(AssignmentDbContext).Assembly.FullName)));

            services.AddScoped<IAssignmentDbContext>(provider => provider.GetRequiredService<AssignmentDbContext>());

            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailSender, DevEmailSender>();

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<AssignmentDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.AddConsumer<AutoAssignmentConsumer>();
                x.AddConsumer<SlaViolationConsumer>();
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

            services.AddHostedService<SlaMonitorWorker>();

            return services;
        }

        public static void EnsureLeadAssignmentDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating LeadAssignment DB: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}
