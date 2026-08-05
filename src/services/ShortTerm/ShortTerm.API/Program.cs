using ShortTerm.Domain.Enums;
using Customer.Domain.Enums;
using Shared.Contracts.Enums;
using Shared.Authentication;
using ShortTerm.Infrastructure;
using ShortTerm.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ── CRM Branch Infrastructure: ShortTermDb ──
builder.Services.AddShortTermInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Shared.Common.Converters.EnumDescriptionJsonConverterFactory());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<Shared.Common.Swagger.EnumDescriptionSchemaFilter>();
    c.SwaggerDoc("v1", new() { Title = "CRM Admissions — ShortTerm API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Vui lòng nhập Token (chỉ copy token, không cần gõ chữ Bearer)",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFiles = System.IO.Directory.GetFiles(AppContext.BaseDirectory, "*.xml", System.IO.SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        try { c.IncludeXmlComments(xmlFile); } catch { }
    }
});


var app = builder.Build();
app.UseMiddleware<Shared.Common.Middleware.GlobalExceptionHandlerMiddleware>();

app.Services.EnsureShortTermDatabaseCreated();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/shortterm/swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/shortterm/swagger/v1/swagger.json", "ShortTerm API");
        c.RoutePrefix = "api/shortterm/swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

