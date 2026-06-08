# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copiar archivos de solución y proyectos primero (cache de capas)
COPY IaAgent.sln .
COPY src/IaAgent.Domain/IaAgent.Domain.csproj src/IaAgent.Domain/
COPY src/IaAgent.Application/IaAgent.Application.csproj src/IaAgent.Application/
COPY src/IaAgent.Infrastructure/IaAgent.Infrastructure.csproj src/IaAgent.Infrastructure/
COPY src/IaAgent.Worker/IaAgent.Worker.csproj src/IaAgent.Worker/

RUN dotnet restore src/IaAgent.Worker/IaAgent.Worker.csproj

# Copiar todo el código fuente
COPY src/ src/

# Publicar la aplicación
RUN dotnet publish src/IaAgent.Worker/IaAgent.Worker.csproj \
    -c Release \
    -o /app \
    --no-restore

# Etapa 2: Runtime (imagen mínima)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Crear directorios necesarios
RUN mkdir -p /data /app/logs

# Usuario no-root por seguridad
RUN groupadd --gid 1000 appgroup && \
    useradd --uid 1000 --gid appgroup --shell /bin/bash --create-home appuser && \
    chown -R appuser:appgroup /app /data

COPY --from=build --chown=appuser:appgroup /app .

USER appuser

ENTRYPOINT ["dotnet", "IaAgent.Worker.dll"]
