# ICAP Juvenil — Backend (.NET 8)

Backend completo siguiendo **Clean Architecture + DDD**, con las 4 capas
especificadas en el Documento de Diseño del Sistema.

```
backend/
├── IcapJuvenil.sln
├── docker-compose.yml               ← levanta MongoDB + la API en un comando
├── src/
│   ├── Icap.Domain/                 ← Core del dominio (sin dependencias externas)
│   ├── Icap.Application/            ← Casos de uso (CQRS vía MediatR + FluentValidation)
│   ├── Icap.Infrastructure/         ← MongoDB.Driver, JWT, hashing, folios, QR hash, seed
│   └── Icap.WebApi/                 ← Controllers, middleware, Swagger, Program.cs
└── tests/
    ├── Icap.Domain.Tests/           ← xUnit: Value Objects y Aggregates
    └── Icap.Application.Tests/      ← xUnit + Moq: Handlers, Validators, pipeline
```

## Icap.Domain

- **Aggregates**: `User`, `Receipt` (con factory methods `Create`/`Issue` para
  altas nuevas, y `Rehydrate` para que Infrastructure reconstruya el estado
  persistido sin usar reflection ni exponer setters públicos).
- **Sub-entidades embebidas**: `DelegateInfo`, `PurchaseDetails`, `ReceiptValidation`.
- **Value Objects**: `Money`, `Email`.
- **Interfaces**: `IUserRepository`, `IReceiptRepository`, `IPasswordHasher`,
  `IJwtTokenGenerator`, `IQrHashGenerator`, `IFolioNumberGenerator`.
- Cero paquetes NuGet: `Icap.Domain.csproj` no referencia nada externo.

## Icap.Application

CQRS con MediatR: cada caso de uso vive en su propia carpeta con
`Command`/`Query` + `Handler` + `Validator` (FluentValidation, ejecutado
automáticamente por `ValidationBehavior`).

- `Auth/Commands/Login` → `LoginQuery` (login y emisión de JWT).
- `Receipts/Commands/CreateReceipt`, `CancelReceipt`.
- `Receipts/Queries/GetReceiptById`, `GetReceipts`.
- `Common/Exceptions`: `ValidationException`, `NotFoundException`,
  `UnauthorizedAccessAppException` — el middleware de `Icap.WebApi` las
  traduce a 400/404/401 respectivamente.

## Icap.Infrastructure

- **Persistence**: `MongoDbContext` (driver oficial `MongoDB.Driver`),
  `UserDocument`/`ReceiptDocument` (modelos BSON separados del dominio a
  propósito) y sus `Mappers` hacia/desde los Aggregates.
- **Security**: `PasswordHasher` (BCrypt), `JwtTokenGenerator`
  (`System.IdentityModel.Tokens.Jwt`, incluye claim de rol para
  `[Authorize(Roles = ...)]`), `QrHashGenerator` (HMAC-SHA256 sobre
  folio+id, firmado con la misma clave del JWT).
- **Services**: `FolioNumberGenerator` — contador atómico (`$inc` sobre la
  colección `Counters`) para folios tipo `ICAP-000123` sin condiciones de
  carrera bajo concurrencia.
- **`DbSeeder`**: seed idempotente (no hace nada si `Users` ya tiene datos)
  que crea un Admin y un Delegado de ejemplo al arrancar. Ver sección
  "Seed inicial" más abajo.
- `DependencyInjection.AddInfrastructure(configuration)` registra todo.

## Icap.WebApi

- **Controllers**: `AuthController` (`POST /api/v1/auth/login`, público) y
  `ReceiptsController` (`GET/POST /api/v1/receipts`, `GET /api/v1/receipts/{id}`,
  `POST /api/v1/receipts/{id}/cancel` — restringido a rol `Admin`).
  Todos requieren JWT vía `[Authorize]`, salvo login.
- **Middleware**: `ExceptionHandlingMiddleware` traduce excepciones de
  Domain/Application a códigos HTTP consistentes (400/401/404/500) con un
  cuerpo JSON uniforme.
- **`ICurrentUserService`**: implementado en `Services/CurrentUserService.cs`
  a partir del `HttpContext.User` poblado por el JWT — así el
  `ReceiptsController` decide "Admin ve todos / Delegate ve los suyos" sin
  que `Icap.Application` conozca ASP.NET Core.
- Swagger con soporte de Bearer token (`Extensions/SwaggerServiceExtensions.cs`).
- CORS habilitado para `http://localhost:4200` (Angular dev server),
  configurable en `appsettings.json` → `Cors:AllowedOrigins`.

## Seed inicial

`Program.cs` ejecuta `DbSeeder.SeedAsync` en cada arranque cuando
`Seed:EnableInitialSeed` es `true` (ya viene así en
`appsettings.Development.json` y en `docker-compose.yml`; en
`appsettings.json` — el de "producción" — viene en `false` a propósito).
Es idempotente: si la colección `Users` ya tiene al menos un documento, no
hace nada. Crea estos dos usuarios de ejemplo:

| Rol      | Email                          | Contraseña     |
|----------|---------------------------------|----------------|
| Admin    | `admin@icapjuvenil.org`         | `Admin123!`    |
| Delegate | `delegado@icapjuvenil.org`      | `Delegado123!` |

**Cámbialas o borra el seed antes de ir a producción.** Para desactivarlo,
pon `Seed:EnableInitialSeed` en `false` (o quita esa sección de
`appsettings.Development.json`).

## Pruebas (xUnit)

```bash
cd backend
dotnet test
```

- **`Icap.Domain.Tests`**: `Money`, `Email` (validación, normalización,
  operaciones), y los Aggregates `User`/`Receipt` (factory methods,
  invariantes de negocio como "no cancelar dos veces", `Rehydrate`).
- **`Icap.Application.Tests`**: cada Handler (`LoginQuery`,
  `CreateReceiptCommand`, `CancelReceiptCommand`, `GetReceiptById`,
  `GetReceipts`) probado con Moq sobre las interfaces del dominio (sin
  MongoDB real), sus Validators de FluentValidation, y el
  `ValidationBehavior` del pipeline de MediatR.
- No incluye pruebas de integración de `Icap.Infrastructure`/`Icap.WebApi`
  (requieren una instancia real o un contenedor efímero de MongoDB —
  ver "Próximos pasos").

## Configuración sensible

`appsettings.json` trae un `JwtSettings:Secret` de ejemplo — **reemplázalo**
antes de correr en cualquier ambiente real, idealmente con `dotnet user-secrets`
en desarrollo y variables de entorno / un secret manager en producción:

```bash
cd src/Icap.WebApi
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "una-clave-larga-y-aleatoria"
```

## Cómo compilar y correr

```bash
cd backend
dotnet restore
dotnet build

# Requiere una instancia de MongoDB corriendo en localhost:27017
dotnet run --project src/Icap.WebApi
# → Swagger disponible en https://localhost:5001/swagger
```

### Con Docker (API + MongoDB en un solo comando)

```bash
cd backend
docker compose up --build
# → API en http://localhost:8080/swagger
```

## Próximos pasos sugeridos (fuera de este alcance)

- Tests de integración de `Icap.WebApi` (`WebApplicationFactory<Program>` +
  Mongo real o Testcontainers) que ejerciten el pipeline HTTP completo.
- Endpoint/UI para gestionar delegados (alta, baja, cambio de contraseña) en
  vez de depender solo del seed.
- Paginación en `GET /api/v1/receipts` si el volumen de recibos crece.

> **Nota de verificación**: este código se generó y revisó cuidadosamente,
> pero no pudo compilarse en el entorno donde se generó por no tener acceso
> de red a NuGet. Al primer `dotnet restore`/`dotnet build` en tu máquina
> revisa la consola por si aparece algún error de referencia menor.
