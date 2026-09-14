using Microsoft.OpenApi.Models;

namespace Icap.WebApi.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddIcapSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ICAP Juvenil API",
                Version = "v1",
                Description = "API para la emisión y control de recibos de la Plataforma ICAP Juvenil.",
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa el token JWT obtenido en /api/v1/auth/login (sin el prefijo 'Bearer ').",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() },
            });
        });

        return services;
    }
}
