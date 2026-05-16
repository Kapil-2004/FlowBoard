using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(int.Parse(port)); });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        handler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true;
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlowBoard API Gateway", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway Service");

    // Use absolute URLs to fetch definitions directly from each service
    c.SwaggerEndpoint("https://auth-service-2izy.onrender.com/swagger/v1/swagger.json", "Auth Service");
    c.SwaggerEndpoint("https://workspace-service-hnf7.onrender.com/swagger/v1/swagger.json", "Workspace Service");
    c.SwaggerEndpoint("https://board-service-9fpw.onrender.com/swagger/v1/swagger.json", "Board Service");
    c.SwaggerEndpoint("https://list-service-e5uu.onrender.com/swagger/v1/swagger.json", "List Service");
    c.SwaggerEndpoint("https://card-service-svau.onrender.com/swagger/v1/swagger.json", "Card Service");
    c.SwaggerEndpoint("https://comment-service-4xaq.onrender.com/swagger/v1/swagger.json", "Comment Service");
    c.SwaggerEndpoint("https://label-service-de13.onrender.com/swagger/v1/swagger.json", "Label Service");
    c.SwaggerEndpoint("https://notification-service-7mxn.onrender.com/swagger/v1/swagger.json", "Notification Service");

    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapReverseProxy();

app.MapGet("/", () => "FlowBoard API Gateway is running!");

app.Run();
