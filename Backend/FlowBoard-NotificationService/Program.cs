using System.Text;
using FlowBoard_NotificationService.Consumers;
using FlowBoard_NotificationService.Data;
using FlowBoard_NotificationService.DTOs;
using FlowBoard_NotificationService.Hubs;
using FlowBoard_NotificationService.Repositories;
using FlowBoard_NotificationService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(10000); });

// ── Controllers ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger ───────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlowBoard Notification Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Database ──────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres"))
{
    var regex = new System.Text.RegularExpressions.Regex(@"postgres(?:ql)?://([^:]+):([^@]+)@([^:/]+)(?::(\d+))?/(.+)");
    var match = regex.Match(connectionString);
    if (match.Success)
    {
        var user = match.Groups[1].Value;
        var pass = match.Groups[2].Value;
        var host = match.Groups[3].Value;
        var port = match.Groups[4].Success ? match.Groups[4].Value : "5432";
        var dbPart = match.Groups[5].Value.Split('?')[0];
        connectionString = $"Host={host};Port={port};Database={dbPart};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true";
    }
}

// Append additional parameters (like SearchPath) from environment variables if provided
var append = builder.Configuration["ConnectionStrings:DefaultConnectionAppend"] ?? ";SearchPath=notification";
connectionString += append;

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "notification")));

// ── Repository & Service ─────────────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();

// ── SignalR ───────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── MassTransit + RabbitMQ ────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationEventConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitUri = builder.Configuration["RabbitMQ:Uri"];
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            cfg.Host(new Uri(rabbitUri));
        }
        else
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });
        }

        cfg.ReceiveEndpoint("notification-events", e =>
        {
            e.ConfigureConsumer<NotificationEventConsumer>(ctx);
        });
    });
});

// ── CORS ─────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:4200")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToList();

        var variations = origins.Select(o => o.TrimEnd('/')).ToList();
        variations.AddRange(variations.Select(v => v + "/").ToList());

        policy.WithOrigins(variations.Distinct().ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();   // Required for SignalR WebSocket upgrade
    });
});

// ── JWT Auth ──────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "FlowBoard-Super-Secret-Key-Must-Be-32-Chars!!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer   = true,
            ValidIssuer      = jwtSettings["Issuer"]   ?? "FlowBoard.Auth",
            ValidateAudience = true,
            ValidAudience    = jwtSettings["Audience"] ?? "FlowBoard.Client",
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero
        };

        // Allow SignalR to authenticate via query-string token
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var access = ctx.Request.Query["access_token"];
                var path   = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(access) && path.StartsWithSegments("/hubs"))
                    ctx.Token = access;
                return Task.CompletedTask;
            }
        };
    });

// ─────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseSwagger(c =>
{
    c.RouteTemplate = "api/notifications/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/notifications/swagger/v1/swagger.json", "FlowBoard Notification API V1");
    c.RoutePrefix = "api/notifications/swagger"; 
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS notification;");
    db.Database.Migrate();
}

app.Run();
