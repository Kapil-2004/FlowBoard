FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowBoard-NotificationService.csproj", "./"]
RUN dotnet restore "FlowBoard-NotificationService.csproj"
COPY . .
RUN dotnet build "FlowBoard-NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowBoard-NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowBoard-NotificationService.dll"]
