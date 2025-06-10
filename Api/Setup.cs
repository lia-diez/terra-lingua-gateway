using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public static class Setup
{
    public static void Swagger(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new() { Title = "MapGen Gateway", Version = "v1" });

        // options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        // {
        //     Description = "API Key needed to access the endpoints",
        //     In = ParameterLocation.Header,
        //     Name = "X-API-KEY",
        //     Type = SecuritySchemeType.ApiKey,
        //     Scheme = "ApiKeyScheme"
        // });

        options.AddSecurityDefinition("Bearer", new
            OpenApiSecurityScheme
            {
                Description = "JWT Bearer Token",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

        // options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }, In = ParameterLocation.Header, }, Array.Empty<string>() } });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }, In = ParameterLocation.Header, }, Array.Empty<string>() } });
    }
}