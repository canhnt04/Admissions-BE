using LeadAssignment.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LeadAssignment.API.Security
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddCustomSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration.GetSection("AppSettings:AccessToken").Value
                ?? throw new InvalidOperationException("AppSettings:AccessToken is missing in configuration.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

            services.AddAuthorization(options =>
            {
                // Cho phép người dùng có Role = Admin, hoặc Role = 3 (EntryClerk/Tư vấn viên), hoặc thuộc Team CustomerCare (RoleTeam = 3)
                options.AddPolicy("RequireCustomerCareOrAdmin", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") ||
                        context.User.IsInRole("EntryClerk") ||
                        context.User.HasClaim(c => c.Type == "RoleTeam" && c.Value == "3")
                    ));

                // Chỉ cho phép Admin (Role = Admin)
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireRole("Admin"));
            });

            return services;
        }
    }
}
