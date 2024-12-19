# Base stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 8083
EXPOSE 8084
EXPOSE 1433

# Install OpenSSL
RUN apt-get update && apt-get install -y openssl

# Generate certificates only if they don't exist, adding SAN extension
RUN mkdir -p /app/certs && \
    if [ ! -f /app/certs/aspnetapp.pfx ]; then \
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /app/certs/aspnetapp.key -out /app/certs/aspnetapp.crt -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost,IP:127.0.0.1" && \
        openssl pkcs12 -export -out /app/certs/aspnetapp.pfx -inkey /app/certs/aspnetapp.key -in /app/certs/aspnetapp.crt -password pass:your_password && \
        chmod 644 /app/certs/aspnetapp.pfx; \
    fi

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY ["src/DomainDrivenWebApplication.Gateway/DomainDrivenWebApplication.Gateway.csproj", "DomainDrivenWebApplication.Gateway/"]
RUN dotnet restore "DomainDrivenWebApplication.Gateway/DomainDrivenWebApplication.Gateway.csproj"

COPY . .

WORKDIR "/src/DomainDrivenWebApplication.Gateway"
RUN dotnet build "DomainDrivenWebApplication.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DomainDrivenWebApplication.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Ensure the application runs as the 'app' user
RUN chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "DomainDrivenWebApplication.Gateway.dll"]