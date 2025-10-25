using Microsoft.EntityFrameworkCore;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure;
using OnlineShop.API.Middleware;
using OnlineShop.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add PostgreSQL logging after getting connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProcessId()
        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/log-.txt", 
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.PostgreSQL(connectionString, "logs", 
            needAutoCreateTable: true)
        .CreateLogger();
}




builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS configuration for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000", 
                "http://localhost:5500",
                "http://127.0.0.1:5500",
                "http://localhost:8080",
                "http://127.0.0.1:8080",
                "http://localhost:8000",
                "http://127.0.0.1:8000",
                "http://localhost:5000",
                "http://127.0.0.1:5000",
                "file://"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    
    // Fallback for development
    options.AddPolicy("DefaultCors", policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var secret = jwtSection["Secret"];
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? string.Empty));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Shop API V1");
        c.RoutePrefix = string.Empty; 
    });

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Only run migrations if using a relational database provider (not InMemory)
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            db.Database.Migrate();
        }
        
        // Seed roles
        await OnlineShop.Infrastructure.Data.DatabaseSeeder.SeedRolesAsync(scope.ServiceProvider);
    }
}


// Use specific CORS policy for frontend, fallback to default for development
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowFrontend");
}
else
{
    app.UseCors("DefaultCors");
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<OnlineShop.WebAPI.Middlewares.RequestLoggingMiddleware>();

// Serve static files from wwwroot
app.UseStaticFiles();

// Serve default files (index.html) for SPA routing
app.UseDefaultFiles();

// app.UseHttpsRedirection(); // Disabled for HTTP-only development

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }

