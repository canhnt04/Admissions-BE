using Customer.Application;
using Customer.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authentication;
using Shared.Common.Middleware;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Shared.Common.Converters.EnumDescriptionJsonConverterFactory());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SchemaFilter<Shared.Common.Swagger.EnumDescriptionSchemaFilter>();
});

// Setup layers
builder.Services.AddCustomerApplication();
builder.Services.AddCustomerInfrastructure(builder.Configuration);

// Add shared Auth
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/customers/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/customers/swagger/v1/swagger.json", "Customer API");
        c.RoutePrefix = "api/customers/swagger";
    });
}

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Ensure DB is created
app.Services.EnsureCustomerDatabaseCreated();

app.Run();
