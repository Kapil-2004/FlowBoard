# FlowBoard Auth Service – Dockerfile
# ─────────────────────────────────────────────────────────────────────────────
# Multi-stage build:
#   Stage 1 (build)  – Restore & compile with the full .NET SDK image
#   Stage 2 (runtime)– Run with the smaller ASP.NET runtime image
#
# Result: ~250 MB final image instead of ~900 MB with SDK included.
# ─────────────────────────────────────────────────────────────────────────────

# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first to leverage Docker layer caching.
# Dependencies are restored in isolation – this layer is rebuilt only when
# the .csproj changes, not on every source code change.
COPY ["FlowBoard-Auth.csproj", "./"]
RUN dotnet restore "FlowBoard-Auth.csproj"

# Copy the full source and build in Release mode
COPY . .
RUN dotnet build "FlowBoard-Auth.csproj" -c Release -o /app/build

# ── Stage 2: Publish ────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "FlowBoard-Auth.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 3: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user for security best practice
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copy published output from the publish stage
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Expose the service port (matches appsettings.json "Urls")
EXPOSE 5001

# Environment variables that can be overridden at runtime:
#   DB_CONNECTION_STRING – PostgreSQL connection string
#   JWT__SECRET          – JWT signing secret (note double underscore for nested config)
#   JWT__ISSUER          – JWT issuer
#   JWT__AUDIENCE        – JWT audience
#   ASPNETCORE_ENVIRONMENT – Development | Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

ENTRYPOINT ["dotnet", "FlowBoard-Auth.dll"]
