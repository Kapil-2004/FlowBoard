# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["FlowBoard-Board.csproj", "./"]
RUN dotnet restore "./FlowBoard-Board.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "FlowBoard-Board.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

ENTRYPOINT ["dotnet", "FlowBoard-Board.dll"]
