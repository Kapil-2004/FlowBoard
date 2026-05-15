# FlowBoard Startup Script for Windows
# This script starts all infrastructure (Docker) and all microservices (Local)

Write-Host "🚀 Starting FlowBoard Infrastructure (Docker)..." -ForegroundColor Cyan
docker-compose up -d

Write-Host "⏳ Waiting for infrastructure to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

$services = @(
    @{ name = "AuthService"; path = "Backend/FlowBoard-Auth"; port = 5001 },
    @{ name = "WorkspaceService"; path = "Backend/FlowBoard-Workspace"; port = 5002 },
    @{ name = "BoardService"; path = "Backend/FlowBoard-Board"; port = 5003 },
    @{ name = "ListService"; path = "Backend/FlowBoard-ListService"; port = 5004 },
    @{ name = "CardService"; path = "Backend/FlowBoard-CardService"; port = 5005 },
    @{ name = "CommentService"; path = "Backend/FlowBoard-Comment_AttachmentService"; port = 5006 },
    @{ name = "LabelService"; path = "Backend/FlowBoard-LabelService"; port = 5007 },
    @{ name = "NotificationService"; path = "Backend/FlowBoard-NotificationService"; port = 5008 },
    @{ name = "ApiGateway"; path = "Backend/FlowBoard-Gateway"; port = 5000 }
)

foreach ($service in $services) {
    Write-Host "Starting $($service.name) on port $($service.port)..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $($service.path); dotnet run"
}

Write-Host "Starting Frontend (Angular)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd Frontend; npm start"

Write-Host "✅ All services are starting in separate windows!" -ForegroundColor Green
Write-Host "Gateway Swagger: http://localhost:5000/swagger" -ForegroundColor White
