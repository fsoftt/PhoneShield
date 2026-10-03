# Build context: repository root.  docker build -f src/Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Domain/Tranqui.Domain.csproj src/Domain/
COPY src/Application/Tranqui.Application.csproj src/Application/
COPY src/Infrastructure/Tranqui.Infrastructure.csproj src/Infrastructure/
COPY src/Contracts/Tranqui.Contracts.csproj src/Contracts/
COPY src/Api/Tranqui.Api.csproj src/Api/
RUN dotnet restore src/Api/Tranqui.Api.csproj

COPY src/Domain/ src/Domain/
COPY src/Application/ src/Application/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Contracts/ src/Contracts/
COPY src/Api/ src/Api/
RUN dotnet publish src/Api/Tranqui.Api.csproj --configuration Release --no-restore --output /app

# Chiseled: no shell, no package manager, runs as a non-root user by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tranqui.Api.dll"]
