using System.Text;
using FlowBoard.Auth.Data;
using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Interfaces;
using FlowBoard.Auth.Repositories;
using FlowBoard.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ═══════════════════════════════════════════════════════════════════════════
//  FlowBoard – Auth Service (UC1)
//  Namespace : FlowBoard.Auth
//  Database  : PostgreSQL  →  flowboard_auth
//  Port      : 5001 (local) | configured via ASPNETCORE_URLS in Docker
// ═══════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(int.Parse(port)); });

// ── 1. Configuration ─────────────────────────────────────────────────────
// Bind strongly-typed JwtSettings from appsettings.json → "Jwt" section
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JWT settings are missing from appsettings.json");

// ── 2. Database – PostgreSQL via EF Core ─────────────────────────────────
// Connection string is read from environment variable first, then appsettings.
// This allows Docker / Render to inject the real connection string at runtime.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                       ?? throw new InvalidOperationException("Database connection string not configured.");

if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("://"))
{
    try 
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        
        connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        Console.WriteLine($"[INFO] Successfully parsed Render connection string for host: {host}");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to parse database connection string: {ex.Message}", ex);
    }
}


// Append additional parameters (like SearchPath) from environment variables if provided
var append = builder.Configuration["ConnectionStrings:DefaultConnectionAppend"] ?? ";SearchPath=auth";
connectionString += append;

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

// ── 3. DI Registrations ────────────────────────────────────────────────
// Repository (scoped – one per request, tied to DbContext lifetime)
builder.Services.AddScoped<UserRepository>();

// Services
builder.Services.AddScoped<IAuthService,  AuthService>();
builder.Services.AddScoped<IJwtService,   JwtService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

// Named HttpClient for OAuth provider calls (Google userinfo, GitHub API)
builder.Services.AddHttpClient("oauth");

// Generic HttpClient for AdminController proxy calls to other microservices
builder.Services.AddHttpClient();

// ── 4. Authentication – JWT Bearer ────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidateAudience         = true,
        ValidAudience            = jwtSettings.Audience,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero // no grace period for expired tokens
    };
});

builder.Services.AddAuthorization(options =>
{
    // Any authenticated user
    options.AddPolicy("AuthenticatedUser", p => p.RequireAuthenticatedUser());

    // Regular members and above
    options.AddPolicy("MemberOrAbove",
        p => p.RequireRole("Member", "BoardAdmin", "PlatformAdmin"));

    // Board administrators and platform admins
    options.AddPolicy("BoardAdminOrAbove",
        p => p.RequireRole("BoardAdmin", "PlatformAdmin"));

    // Platform admin only
    options.AddPolicy("PlatformAdminOnly",
        p => p.RequireRole("PlatformAdmin"));
});

// ── 5. Controllers ────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 6. Swagger / OpenAPI ──────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FlowBoard – Auth Service",
        Version     = "v1",
        Description = "UC1: User registration, login (JWT), OAuth (Google/GitHub), and profile management."
    });

    // Add "Authorize" button in Swagger UI to send Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── 7. CORS ───────────────────────────────────────────────────────────
// Allow Angular frontend (and API Gateway) to call this service.
// Update the origin list to match your deployed frontend URL.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:4200")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToList();

        // Add versions with/without trailing slashes
        var variations = origins.Select(o => o.TrimEnd('/')).ToList();
        variations.AddRange(variations.Select(v => v + "/").ToList());

        policy.WithOrigins(variations.Distinct().ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 8. Auto-migrate on startup ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        if (db.Database.IsRelational())
        {
            db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS auth;");
            db.Database.Migrate();
            app.Logger.LogInformation("Database migration applied successfully.");
        }
        else
        {
            app.Logger.LogInformation("Non-relational database detected. Skipping migrations.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed. Starting without migration.");
    }
}

// ── 9. Middleware Pipeline ─────────────────────────────────────────────
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard Auth Service v1");
    c.RoutePrefix = "swagger"; // Explicitly set to 'swagger'
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "FlowBoard Auth Service is running!");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
