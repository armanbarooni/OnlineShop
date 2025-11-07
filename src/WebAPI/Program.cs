using Microsoft.EntityFrameworkCore;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure;
using OnlineShop.API.Middleware;
using OnlineShop.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.DataProtection;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;
var isDevelopmentEnvironment = environmentName.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase);
var minimumLogLevel = isDevelopmentEnvironment ? LogEventLevel.Debug : LogEventLevel.Information;

LoggerConfiguration BuildLoggerConfiguration(LogEventLevel minimumLevel, string? postgresConnectionString = null)
{
    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Is(minimumLevel)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProcessId()
        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

    if (!string.IsNullOrEmpty(postgresConnectionString))
    {
        loggerConfiguration = loggerConfiguration.WriteTo.PostgreSQL(
            postgresConnectionString,
            "logs",
            needAutoCreateTable: true);
    }

    return loggerConfiguration;
}

// Configure Serilog
Log.Logger = BuildLoggerConfiguration(minimumLogLevel).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add PostgreSQL logging after getting connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    Log.Logger = BuildLoggerConfiguration(minimumLogLevel, connectionString).CreateLogger();
}



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Localization (set default culture to fa-IR, support fa and en)
var supportedCultures = new[] { new System.Globalization.CultureInfo("fa-IR"), new System.Globalization.CultureInfo("en-US") };
builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("fa-IR");
    options.SupportedCultures = supportedCultures.ToList();
    options.SupportedUICultures = supportedCultures.ToList();
});

var frontendOrigins = builder.Configuration.GetSection("Cors:AllowFrontend").Get<string[]>() ?? Array.Empty<string>();

// CORS configuration for Frontend
builder.Services.AddCors(options =>
{
    // Main policy for frontend - with credentials support
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (frontendOrigins.Length == 0)
        {
            Log.Warning("No frontend origins configured in Cors:AllowFrontend; falling back to allow any origin without credentials.");
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins(frontendOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });

    // Fallback for development - allow all origins (but without credentials)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("DefaultCors", policy =>
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    }
});

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var secret = jwtSection["Secret"];
var secretBytes = string.IsNullOrWhiteSpace(secret) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(secret);
if (secretBytes.Length < 32)
{
    Log.Warning("JWT secret is not configured or shorter than 32 bytes. Ensure JWT__SECRET environment variable is set in production.");
}
Log.Information("Loaded JWT settings for {Environment}: Issuer='{Issuer}', Audience='{Audience}', SecretLength={SecretLength}",
    builder.Environment.EnvironmentName,
    string.IsNullOrWhiteSpace(issuer) ? "<empty>" : issuer,
    string.IsNullOrWhiteSpace(audience) ? "<empty>" : audience,
    secretBytes.Length);
var key = new SymmetricSecurityKey(secretBytes.Length == 0 ? Encoding.UTF8.GetBytes("development-secret-placeholder-change-me") : secretBytes);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Configure DataProtection for Production (persist keys to disk instead of in-memory)
if (!builder.Environment.IsDevelopment())
{
    var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("OnlineShop");
}

var app = builder.Build();

var shouldSeedDefaults = !string.Equals(
    Environment.GetEnvironmentVariable("SEED_DEFAULT_USERS"),
    "false",
    StringComparison.OrdinalIgnoreCase);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Shop API V1");
        c.RoutePrefix = string.Empty;
    });
}

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            db.Database.Migrate();
        }

        if (shouldSeedDefaults)
        {
            await OnlineShop.Infrastructure.Data.DatabaseSeeder.SeedRolesAsync(scope.ServiceProvider);
        }
    }
}


// CORS must be before UseAuthentication and UseAuthorization
// CORS middleware must be called BEFORE UseAuthentication/UseAuthorization
app.UseCors(app.Environment.IsDevelopment() ? "DefaultCors" : "AllowFrontend");

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<OnlineShop.WebAPI.Middlewares.RequestLoggingMiddleware>();

// Apply request localization
app.UseRequestLocalization();

// Serve default files (index.html) for SPA routing - MUST be before UseStaticFiles
app.UseDefaultFiles();

// Serve static files from wwwroot - MUST be after UseDefaultFiles
app.UseStaticFiles();

// HTTPS Redirection - only enable if HTTPS is properly configured
// On IIS with HTTPS binding, this will work automatically
// For HTTP-only deployments, this causes warnings but won't break functionality
if (!app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("EnableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Ensure UTF-8 encoding and Persian language headers where applicable
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var acceptLang = context.Request.Headers["Accept-Language"].ToString();

        if (context.Response.ContentType?.StartsWith("application/json") == true)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
        }
        else if (context.Response.ContentType?.StartsWith("text/html") == true)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
        }

        if (path.StartsWith("/fa", StringComparison.OrdinalIgnoreCase) || acceptLang.StartsWith("fa", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers["Content-Language"] = "fa-IR";
        }
        return Task.CompletedTask;
    });
    await next();
});

// Inject global RTL CSS/meta for Farsi pages
app.UseMiddleware<OnlineShop.WebAPI.Middlewares.RtlLocalizationMiddleware>();

app.MapControllers();


app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
