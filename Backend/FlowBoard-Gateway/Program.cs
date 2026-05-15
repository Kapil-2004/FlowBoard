using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(10000); });

// Add services to the container.
builder.Services.AddControllers();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlowBoard API Gateway", Version = "v1" });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");
    
    // Aggregated Swagger endpoints using the Gateway's OWN proxy routes.
    // This bypasses CORS issues because the browser thinks it's staying on localhost:5000.
    c.SwaggerEndpoint("/api/auth/swagger/v1/swagger.json", "Auth Service");
    c.SwaggerEndpoint("/api/workspaces/swagger/v1/swagger.json", "Workspace Service");
    c.SwaggerEndpoint("/api/boards/swagger/v1/swagger.json", "Board Service");
    c.SwaggerEndpoint("/api/lists/swagger/v1/swagger.json", "List Service");
    c.SwaggerEndpoint("/api/cards/swagger/v1/swagger.json", "Card Service");
    c.SwaggerEndpoint("/api/comments/swagger/v1/swagger.json", "Comment Service");
    c.SwaggerEndpoint("/api/labels/swagger/v1/swagger.json", "Label Service");
    c.SwaggerEndpoint("/api/notifications/swagger/v1/swagger.json", "Notification Service");
    
    c.RoutePrefix = "swagger";
});

app.UseCors();
app.UseRouting();

app.MapReverseProxy();

app.Run();
