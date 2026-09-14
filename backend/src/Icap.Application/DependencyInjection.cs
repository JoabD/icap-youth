using System.Reflection;
using FluentValidation;
using Icap.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Icap.Application;

/// <summary>
/// Punto único de registro de la capa Application en el contenedor de DI.
/// La capa Presentation (Program.cs) solo necesita llamar a
/// services.AddApplication() sin conocer los detalles internos (MediatR,
/// FluentValidation, pipeline behaviors).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
