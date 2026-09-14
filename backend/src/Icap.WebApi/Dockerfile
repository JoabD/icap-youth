FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Icap.Domain/Icap.Domain.csproj", "src/Icap.Domain/"]
COPY ["src/Icap.Application/Icap.Application.csproj", "src/Icap.Application/"]
COPY ["src/Icap.Infrastructure/Icap.Infrastructure.csproj", "src/Icap.Infrastructure/"]
COPY ["src/Icap.WebApi/Icap.WebApi.csproj", "src/Icap.WebApi/"]
RUN dotnet restore "src/Icap.WebApi/Icap.WebApi.csproj"

COPY . .
WORKDIR "/src/src/Icap.WebApi"
RUN dotnet build "Icap.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Icap.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Icap.WebApi.dll"]
