using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Shared.Common.Swagger
{
    public class EnumDescriptionSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Type = "string";
                schema.Enum.Clear();
                foreach (var name in Enum.GetNames(context.Type))
                {
                    var field = context.Type.GetField(name);
                    if (field != null)
                    {
                        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                        schema.Enum.Add(new OpenApiString(attribute?.Description ?? name));
                    }
                }
            }
            else if (Nullable.GetUnderlyingType(context.Type)?.IsEnum == true)
            {
                var underlyingType = Nullable.GetUnderlyingType(context.Type);
                schema.Type = "string";
                schema.Enum.Clear();
                foreach (var name in Enum.GetNames(underlyingType!))
                {
                    var field = underlyingType.GetField(name);
                    if (field != null)
                    {
                        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                        schema.Enum.Add(new OpenApiString(attribute?.Description ?? name));
                    }
                }
            }
        }
    }
}
