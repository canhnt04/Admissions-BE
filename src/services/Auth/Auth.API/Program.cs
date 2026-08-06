using Auth.Domain.Enums;
using Shared.Contracts.Enums;
using Shared.Authentication;
using Auth.Application;
using Auth.Application.Events;
using Auth.Infrastructure;
using Auth.Infrastructure.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Auth Infrastructure (AuthDbContext + RabbitMQ publish) ──
builder.Services.AddAuthInfrastructure(builder.Configuration);

// ── Auth Application (MediatR, FluentValidation) ──
builder.Services.AddAuthApplication();
builder.Services.AddCurrentUserService();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Shared.Common.Converters.EnumDescriptionJsonConverterFactory());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<Shared.Common.Swagger.EnumDescriptionSchemaFilter>();
    options.SwaggerDoc("v1", new() { Title = "CRM Admissions — Auth API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Vui lòng nhập Token (chỉ copy token, không cần gõ chữ Bearer)",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.OperationFilter<Auth.API.Infrastructure.Filters.AuthorizeCheckOperationFilter>();

    var xmlFiles = System.IO.Directory.GetFiles(AppContext.BaseDirectory, "*.xml", System.IO.SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        try { options.IncludeXmlComments(xmlFile); } catch { }
    }
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:AccessToken").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
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
                return Task.CompletedTask;
            }
        };
    });


var app = builder.Build();
app.UseMiddleware<Shared.Common.Middleware.GlobalExceptionHandlerMiddleware>();

app.Services.EnsureAuthDatabaseCreated();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/auth/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/auth/swagger/v1/swagger.json", "Auth API");
        c.RoutePrefix = "api/auth/swagger";
    });
}

// app.UseHttpsRedirection();

// Use Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<Auth.API.Services.GrpcUserService>();

app.Run();


