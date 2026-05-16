using Microsoft.OpenApi.Models;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Use the PORT environment variable provided by Render, defaulting to 10000
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.ConfigureKestrel(serverOptions => 
{ 
    serverOptions.ListenAnyIP(int.Parse(port)); 
});

// Add services to the container.
builder.Services.AddControllers();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformContext =>
    {
        // Automatically remove the API prefix based on the Route ID
        var routeId = transformContext.Route.RouteId;
        if (routeId.StartsWith("auth")) transformContext.AddPathRemovePrefix("/api/auth");
        else if (routeId.StartsWith("workspace")) transformContext.AddPathRemovePrefix("/api/workspaces");
        else if (routeId.StartsWith("board")) transformContext.AddPathRemovePrefix("/api/boards");
        else if (routeId.StartsWith("list")) transformContext.AddPathRemovePrefix("/api/lists");
        else if (routeId.StartsWith("card")) transformContext.AddPathRemovePrefix("/api/cards");
        else if (routeId.StartsWith("comment")) transformContext.AddPathRemovePrefix("/api/comments");
        else if (routeId.StartsWith("label")) transformContext.AddPathRemovePrefix("/api/labels");
        else if (routeId.StartsWith("notification")) transformContext.AddPathRemovePrefix("/api/notifications");
    });

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
        var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "https://flowboard-web.onrender.com")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToArray();

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");
    
    // Aggregated Swagger endpoints using the Gateway's OWN proxy routes.
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

// Diagnostic Status Page
app.MapGet("/", (IConfiguration config) => 
{
    var auth = config["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"];
    var workspace = config["ReverseProxy:Clusters:workspace-cluster:Destinations:destination1:Address"];
    
    return Results.Content($@"
        <html>
            <body style='font-family: sans-serif; padding: 40px; background: #f8f9fa; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0;'>
                <div style='background: white; padding: 40px; border-radius: 16px; box-shadow: 0 10px 25px rgba(0,0,0,0.1); max-width: 600px; width: 100%;'>
                    <h1 style='color: #2563eb; margin-top: 0;'>🚀 FlowBoard Gateway Status</h1>
                    <div style='background: #ecfdf5; color: #065f46; padding: 12px; border-radius: 8px; margin-bottom: 24px; font-weight: bold;'>
                        ● Gateway is Live and Healthy
                    </div>
                    <p style='color: #4b5563;'>Listening on Port: <strong>{port}</strong></p>
                    <hr style='border: 0; border-top: 1px solid #e5e7eb; margin: 24px 0;'/>
                    <h3 style='color: #1f2937;'>Backend Clusters:</h3>
                    <ul style='list-style: none; padding: 0;'>
                        <li style='padding: 8px 0;'>🔹 Auth: <code style='background: #f3f4f6; padding: 2px 6px; border-radius: 4px;'>{auth ?? "MISSING"}</code></li>
                        <li style='padding: 8px 0;'>🔹 Workspace: <code style='background: #f3f4f6; padding: 2px 6px; border-radius: 4px;'>{workspace ?? "MISSING"}</code></li>
                    </ul>
                    <p style='color: #6b7280; font-size: 0.85em; margin-top: 24px; line-height: 1.5;'>
                        Tip: Use Internal Render Hostnames (e.g., http://auth-service:10000) for maximum reliability and speed.
                    </p>
                </div>
            </body>
        </html>", "text/html");
});

app.MapReverseProxy();

app.Run();
