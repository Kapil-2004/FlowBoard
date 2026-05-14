FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["FlowBoard-Comment_AttachmentService.csproj", "./"]
RUN dotnet restore "FlowBoard-Comment_AttachmentService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "FlowBoard-Comment_AttachmentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "FlowBoard-Comment_AttachmentService.dll"]
