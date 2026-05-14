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

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

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
        policy.WithOrigins(
                  builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                  ?? new[] { "http://localhost:4200" })
              .AllowAnyHeader()
              .AllowAnyMethod());
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard Auth Service v1");
        c.RoutePrefix = string.Empty; // Swagger at root: http://localhost:5001/
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
