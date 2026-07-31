using Auth.Application.Common.Helpers;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Shared.Common.Extensions;
using Auth.Infrastructure.Messaging.Publishers;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("AuthDatabase"),
                    b => b.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName)));

            services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
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
            services.AddScoped<IUserEventPublisher, UserEventPublisher>();

            return services;
        }

        public static void EnsureAuthDatabaseCreated(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    context.Database.Migrate();

                    // Seed tự động danh sách Teams từ Enum RoleTeam
                    var existingRoleTeams = context.Teams
                        .Where(t => t.RoleTeam.HasValue)
                        .Select(t => t.RoleTeam!.Value)
                        .ToList();

                    foreach (RoleTeam roleTeam in Enum.GetValues(typeof(RoleTeam)))
                    {
                        if (!existingRoleTeams.Contains(roleTeam))
                        {
                            context.Teams.Add(new Team
                            {
                                Id = Guid.NewGuid(),
                                Name = roleTeam.GetDescription(),
                                RoleTeam = roleTeam,
                                IsActive = true
                            });
                        }
                    }

                    if (!context.Users.Any(u => u.UserName == "admin"))
                    {
                         PasswordHelper.CreatePasswordHash("admin", out byte[] passwordHash, out byte[] passwordSalt);
                        context.Users.Add(new Auth.Domain.Entities.User
                        {
                            Id = Guid.NewGuid(),
                            UserName = "admin",
                            PasswordHash = Convert.ToBase64String(passwordHash),  
                            PasswordSalt = passwordSalt,   
                            FullName = "Admin",
                            Role = Auth.Domain.Entities.Role.Admin,
                            Mobile = "",
                            IdentificationNumber = "000000000000",
                            IsActived = true,
                            UserInternalId = "ADMIN001",
                            ProfilePicUrl = "",
                        });
                    }

                    context.SaveChanges();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB INIT] Retry {i + 1}/10 creating AuthDb: {ex.Message}");
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }
    }
}
