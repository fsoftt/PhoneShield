# Build context: repository root.  docker build -f src/BackOffice/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Contracts/Tranqui.Contracts.csproj src/Contracts/
COPY src/BackOffice/Tranqui.BackOffice.csproj src/BackOffice/
RUN dotnet restore src/BackOffice/Tranqui.BackOffice.csproj

COPY src/Contracts/ src/Contracts/
COPY src/BackOffice/ src/BackOffice/
RUN dotnet publish src/BackOffice/Tranqui.BackOffice.csproj --configuration Release --no-restore --output /app \
    && mkdir -p /data-protection

# Chiseled: no shell, no package manager, runs as a non-root user by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
# Owned by the app user so a named volume mounted here starts writable (keys that keep sessions valid across restarts).
COPY --from=build --chown=$APP_UID:$APP_UID /data-protection /data-protection
ENV ASPNETCORE_HTTP_PORTS=8080 \
    BackOffice__DataProtectionKeysPath=/data-protection
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tranqui.BackOffice.dll"]
