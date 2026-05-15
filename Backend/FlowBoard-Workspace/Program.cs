using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(10000); });

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("://"))
{
    try 
    {
        var schemeIndex = connectionString.IndexOf("://");
        var uriPart = connectionString.Substring(schemeIndex + 3);
        var atIndex = uriPart.LastIndexOf('@');
        if (atIndex > 0)
        {
            var credentials = uriPart.Substring(0, atIndex);
            var serverDb = uriPart.Substring(atIndex + 1);
            var userPass = credentials.Split(':');
            if (userPass.Length >= 2)
            {
                var hostDb = serverDb.Split('/');
                var hostPort = hostDb[0].Split(':');
                var dbPart = hostDb.Length > 1 ? hostDb[1].Split('?')[0] : "";
                if (!string.IsNullOrEmpty(dbPart))
                {
                    connectionString = $"Host={hostPort[0]};Port={(hostPort.Length > 1 ? hostPort[1] : "5432")};Database={dbPart};Username={userPass[0]};Password={userPass[1]};SSL Mode=Require;Trust Server Certificate=true";
                }
            }
        }
    }
    catch { }
}

// Append additional parameters (like SearchPath) from environment variables if provided
var append = builder.Configuration["ConnectionStrings:DefaultConnectionAppend"] ?? ";SearchPath=workspace";
connectionString += append;

builder.Services.AddDbContext<FlowBoard_Workspace.Data.WorkspaceDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "workspace")));

// Register Services and Repositories
builder.Services.AddScoped<FlowBoard_Workspace.Interfaces.IWorkspaceRepository, FlowBoard_Workspace.Repositories.WorkspaceRepository>();
builder.Services.AddScoped<FlowBoard_Workspace.Interfaces.IWorkspaceService, FlowBoard_Workspace.Services.WorkspaceServiceImpl>();
builder.Services.AddScoped<FlowBoard_Workspace.Interfaces.IBoardRepository, FlowBoard_Workspace.Repositories.BoardRepository>();
builder.Services.AddScoped<FlowBoard_Workspace.Interfaces.IBoardService, FlowBoard_Workspace.Services.BoardServiceImpl>();

// Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlowBoard Workspace API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // For dev only
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
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

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowBoard_Workspace.Data.WorkspaceDbContext>();
    db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS workspace;");
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard Workspace API V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "FlowBoard Workspace Service is running!");

app.Run();
