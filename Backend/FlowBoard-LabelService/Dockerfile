FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5007

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowBoard-LabelService.csproj", "./"]
RUN dotnet restore "FlowBoard-LabelService.csproj"
COPY . .
RUN dotnet build "FlowBoard-LabelService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowBoard-LabelService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowBoard-LabelService.dll"]
