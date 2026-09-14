using System.Net;
using System.Text.Json;
using Icap.Application.Common.Exceptions;
using Icap.Domain.Exceptions;
using ValidationException = Icap.Application.Common.Exceptions.ValidationException;

namespace Icap.WebApi.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones (ASP.NET Core "clásico", en vez
/// de exception filters, para capturar también errores fuera del pipeline de
/// MVC). Traduce cada tipo de excepción de Domain/Application a un status
/// code HTTP y un cuerpo JSON consistente (problem-details simplificado).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Se encontraron uno o más errores de validación.",
                (object?)validationException.Errors),

            DomainException domainException => (
                HttpStatusCode.BadRequest,
                domainException.Message,
                null),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                null),

            UnauthorizedAccessAppException unauthorizedException => (
                HttpStatusCode.Unauthorized,
                unauthorizedException.Message,
                null),

            _ => (
                HttpStatusCode.InternalServerError,
                "Ocurrió un error inesperado. Intenta de nuevo más tarde.",
                null),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado procesando {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Error de negocio procesando {Path}: {Message}", context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            title,
            errors,
            traceId = context.TraceIdentifier,
        });

        await context.Response.WriteAsync(payload);
    }
}
