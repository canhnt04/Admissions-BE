using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Shared.Common.Interfaces;
using Shared.Authentication.Services;

namespace Shared.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<JwtTokenGenerator>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
                };
                options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.Contains("\"accessToken\""))
                    {
                        try
                        {
                            var jsonPart = authHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase) 
                                ? authHeader.Substring(7).Trim() 
                                : authHeader.Trim();
                            if (jsonPart.StartsWith("{") && jsonPart.EndsWith("}"))
                            {
                                using var doc = System.Text.Json.JsonDocument.Parse(jsonPart);
                                if (doc.RootElement.TryGetProperty("accessToken", out var tokenElement))
                                {
                                    context.Token = tokenElement.GetString();
                                }
                            }
                        }
                        catch { }
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        return services;
    }
}

