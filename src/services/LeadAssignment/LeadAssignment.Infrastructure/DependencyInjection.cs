using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using LeadAssignment.Application.Common.Interfaces;
using LeadAssignment.Infrastructure.Consumers;
using LeadAssignment.Infrastructure.Data;
using LeadAssignment.Infrastructure.Repositories;
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

            // ── Repositories ─────────────────────────────────────────────
            services.AddScoped<IAssignmentQueueRepository, AssignmentQueueRepository>();
            services.AddScoped<ICustomerCareStatusRepository, CustomerCareStatusRepository>();
            services.AddScoped<ICustomerAssignmentHistoryRepository, CustomerAssignmentHistoryRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
            services.AddScoped<IContactEvidenceRepository, ContactEvidenceRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailSender, DevEmailSender>();

            var authServiceUrl = configuration["GrpcConfig:AuthServiceUrl"] ?? "http://auth-api:8080";
            services.AddGrpcClient<Shared.Protos.Users.UserService.UserServiceClient>(o =>
            {
                o.Address = new Uri(authServiceUrl);
            });
            services.AddScoped<IUserGrpcClient, UserGrpcClient>();

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<AssignmentDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.AddConsumer<AutoAssignmentConsumer>();
                x.AddConsumer<SlaViolationConsumer>();

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
