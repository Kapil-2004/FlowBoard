# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cache friendly)
COPY ["FlowBoard-ListService.csproj", "./"]
RUN dotnet restore "./FlowBoard-ListService.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "FlowBoard-ListService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "FlowBoard-ListService.dll"]
