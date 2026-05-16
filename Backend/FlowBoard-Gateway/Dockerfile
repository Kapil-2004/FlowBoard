FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
# Fix for Segmentation Fault (status 139)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowBoard-Gateway.csproj", "./"]
RUN dotnet restore "FlowBoard-Gateway.csproj"
COPY . .
RUN dotnet build "FlowBoard-Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowBoard-Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowBoard-Gateway.dll"]
