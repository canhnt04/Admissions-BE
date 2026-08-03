using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── JWT Authentication ──
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// ── YARP Reverse Proxy ──
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CRM Admissions — API Gateway", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/auth/swagger/v1/swagger.json", "Auth API");
        c.SwaggerEndpoint("/api/formal/swagger/v1/swagger.json", "Formal API");
        c.SwaggerEndpoint("/api/shortterm/swagger/v1/swagger.json", "ShortTerm API");
        c.SwaggerEndpoint("/api/driving/swagger/v1/swagger.json", "Driving API");
        c.SwaggerEndpoint("/api/assignment/swagger/v1/swagger.json", "LeadAssignment API");
        c.SwaggerEndpoint("/api/customers/swagger/v1/swagger.json", "Customer API");
    });
}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map YARP reverse proxy routes
app.MapReverseProxy();

app.Run();
