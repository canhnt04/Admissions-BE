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
                // Cho phép người dùng có Role = Admin, hoặc thuộc Team CustomerCare (3), ShortTerm (4), Formal (5), Driving (6)
                options.AddPolicy("RequireCustomerCareOrAdmin", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") ||
                        context.User.HasClaim(c => c.Type == "RoleTeam" && (c.Value == "4" || c.Value == "5" || c.Value == "6"))
                    ));

                // Chỉ cho phép Admin (Role = Admin)
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireRole("Admin"));
            });

            return services;
        }
    }
}
