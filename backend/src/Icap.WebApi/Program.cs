using System.Text;
using Icap.Application;
using Icap.Domain.Services;
using Icap.Infrastructure;
using Icap.Infrastructure.Persistence;
using Icap.Infrastructure.Security;
using Icap.WebApi.Extensions;
using Icap.WebApi.Middleware;
using Icap.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Registro de capas (cada una expone su propio AddXxx()) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Presentation: MVC, CORS, Swagger, Auth ---
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Icap.Application.Common.Interfaces.ICurrentUserService, CurrentUserService>();

builder.Services.AddIcapSwagger();

const string AngularCorsPolicy = "AngularClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                         ?? new[] { "http://localhost:4200" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations();

// Se lee la sección una sola vez aquí (en vez de resolver un ServiceProvider
// intermedio) para configurar el esquema JwtBearer con la misma fuente de
// verdad (JwtSettings) que usa Icap.Infrastructure para firmar los tokens.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException($"Falta configurar la sección '{JwtSettings.SectionName}' en appsettings.json.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Crea (idempotentemente) los índices únicos requeridos por el diseño
// (Users.Email, Receipts.FolioNumber), y aplica el seed inicial de usuarios
// de ejemplo (ver DbSeeder — no hace nada si Users ya tiene datos) antes de
// aceptar tráfico. El seed se puede desactivar con "Seed:EnableInitialSeed":
// false en appsettings.json (ver appsettings.Development.json, donde va en true).
using (var scope = app.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
    await MongoIndexInitializer.EnsureIndexesAsync(mongoContext);

    if (builder.Configuration.GetValue("Seed:EnableInitialSeed", defaultValue: false))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        await DbSeeder.SeedAsync(mongoContext, passwordHasher, seedLogger);
    }
}

// --- Pipeline HTTP ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ICAP Juvenil API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors(AngularCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para que WebApplicationFactory<Program> funcione en pruebas de integración.
public partial class Program { }
