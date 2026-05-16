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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapReverseProxy();

app.MapGet("/", () => "FlowBoard API Gateway is running!");

app.Run();
