using Icap.Domain.Repositories;
using Icap.Domain.Services;
using Icap.Infrastructure.Email;
using Icap.Infrastructure.Pdf;
using Icap.Infrastructure.Persistence;
using Icap.Infrastructure.Persistence.Repositories;
using Icap.Infrastructure.Persistence.Settings;
using Icap.Infrastructure.Security;
using Icap.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Icap.Infrastructure;

/// <summary>
/// Punto único de registro de la capa Infrastructure. Icap.WebApi (Program.cs)
/// solo necesita llamar a services.AddInfrastructure(configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddSingleton<IMongoDbContext, MongoDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IQrHashGenerator, QrHashGenerator>();
        services.AddScoped<IFolioNumberGenerator, FolioNumberGenerator>();
        services.AddSingleton<IReceiptPdfGenerator, QuestPdfReceiptGenerator>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
