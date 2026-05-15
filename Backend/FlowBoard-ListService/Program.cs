using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FlowBoard_ListService.Data;
using FlowBoard_ListService.Repositories;
using FlowBoard_ListService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(10000); });

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database ─────────────────────────────────────────────────────────────────
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
var append = builder.Configuration["ConnectionStrings:DefaultConnectionAppend"] ?? ";SearchPath=list";
connectionString += append;

builder.Services.AddDbContext<ListDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "list")));

// ── Repositories & Services (UC4) ────────────────────────────────────────────
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<IListService, ListServiceImpl>();

// ── JWT Authentication ────────────────────────────────────────────────────────
// Validates tokens minted by FlowBoard-Auth (shared secret + issuer/audience).
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new InvalidOperationException("JWT Secret is missing in configuration.");

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken            = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidateAudience         = true,
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

// ── CORS ─────────────────────────────────────────────────────────────────────
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
              .AllowCredentials();
    });
});

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FlowBoard List API",
        Version     = "v1",
        Description = "UC4 – List/Column-Service: manages Kanban columns on boards."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. " +
                      "Example: \"Authorization: Bearer {token}\"",
        Name   = "Authorization",
        In     = ParameterLocation.Header,
        Type   = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build & Pipeline ──────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard List API V1");
        c.RoutePrefix = "swagger";
    });
}

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ListDbContext>();
        context.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS list;");
        context.Database.Migrate(); // Auto-migrate on startup
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the List database.");
    }
}

app.UseRouting();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard List API V1");
    // Support Gateway path
    c.SwaggerEndpoint("/api/lists/swagger/v1/swagger.json", "FlowBoard List API V1 (Gateway)");
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
