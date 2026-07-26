using Crm.Application.Common.Behaviors;
using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Consumers;
using Crm.Infrastructure.Data;
using Crm.Infrastructure.Services;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure
{
    /// <summary>
    /// Extension methods đăng ký tất cả services của Infrastructure layer.
    /// Gọi trong Program.cs của mỗi API service.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Database ──
            services.AddDbContext<CrmDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CrmDatabase"),
                    b => b.MigrationsAssembly(typeof(CrmDbContext).Assembly.FullName)));

            // Register DbContext as ICrmDbContext
            services.AddScoped<ICrmDbContext>(provider => provider.GetRequiredService<CrmDbContext>());

            // ── MediatR + FluentValidation ──
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ICrmDbContext).Assembly));

            services.AddValidatorsFromAssembly(typeof(ICrmDbContext).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // ── Application Services ──
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<INotificationService, NotificationService>();

            // ── MassTransit (RabbitMQ) ──
            services.AddMassTransit(x =>
            {
                x.AddConsumer<AutoAssignmentConsumer>();
                x.AddConsumer<SlaViolationConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // ── Background Services ──
            services.AddHostedService<SlaMonitorService>();

            return services;
        }

        public static void EnsureCrmDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.EnsureCreated();
                    if (!context.Users.Any())
                    {
                        Crm.Application.Auth.PasswordHelper.CreatePasswordHash("Password123!", out byte[] hash, out byte[] salt);
                        context.Users.Add(new Crm.Domain.Entities.User
                        {
                            Id = Guid.NewGuid(),
                            UserName = "admin@crm.edu.vn",
                            PasswordHash = Convert.ToBase64String(hash),
                            PasswordSalt = salt,
                            FullName = "Admin CRM",
                            Role = Crm.Domain.Entities.Role.Admin,
                            Mobile = "0901234567",
                            IdentificationNumber = "012345678912",
                            IsActived = true,
                            UserInternalId = "EMP0001",
                            ProfilePicUrl = ""
                        });
                        context.SaveChanges();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating database: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}
