using System.Text;
using FlowBoard_LabelService.Data;
using FlowBoard_LabelService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(int.Parse(port)); });

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlowBoard Label & Checklist Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// Database
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
var append = builder.Configuration["ConnectionStrings:DefaultConnectionAppend"] ?? ";SearchPath=label";
connectionString += append;

builder.Services.AddDbContext<LabelDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "label")));

// Services
builder.Services.AddScoped<ILabelService, LabelServiceImpl>();

// CORS
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

// Auth
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "FlowBoard-Super-Secret-Key-Must-Be-32-Chars!!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "FlowBoard.Auth",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "FlowBoard.Client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowBoard Label API V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "FlowBoard Label Service is running!");

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LabelDbContext>();
    db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS label;");
    db.Database.Migrate();
}

app.Run();
