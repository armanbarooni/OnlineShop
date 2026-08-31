using Microsoft.EntityFrameworkCore;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure;
using OnlineShop.Infrastructure.Security;
using OnlineShop.API.Middleware;
using OnlineShop.Application.Common;
using OnlineShop.WebAPI.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;
var isDevelopmentEnvironment = environmentName.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase);
var isProductionEnvironment = environmentName.Equals(Environments.Production, StringComparison.OrdinalIgnoreCase);
var minimumLogLevel = isDevelopmentEnvironment
    ? LogEventLevel.Debug
    : isProductionEnvironment
        ? LogEventLevel.Error
        : LogEventLevel.Information;
var frameworkLogLevel = isProductionEnvironment ? LogEventLevel.Error : LogEventLevel.Warning;

LoggerConfiguration BuildLoggerConfiguration(LogEventLevel minimumLevel, string? postgresConnectionString = null)
{
    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Is(minimumLevel)
        .MinimumLevel.Override("Microsoft", frameworkLogLevel)
        .MinimumLevel.Override("System", frameworkLogLevel)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProcessId()
        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            restrictedToMinimumLevel: LogEventLevel.Error,
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

if (builder.Environment.IsDevelopment())
{
    // Force the local dev host to use the expected port used across the frontend.
    builder.WebHost.UseUrls("http://localhost:5000");
}

// Use Serilog
builder.Host.UseSerilog();

// Swagger options
var swaggerEnabled = builder.Configuration.GetValue<bool?>("Swagger:Enabled") 
                     ?? builder.Environment.IsDevelopment();
var swaggerRoutePrefixRaw = builder.Configuration.GetValue<string>("Swagger:RoutePrefix");
var swaggerRoutePrefix = swaggerRoutePrefixRaw is null 
    ? "swagger" 
    : swaggerRoutePrefixRaw.Trim('/');
var swaggerAtRoot = swaggerRoutePrefixRaw is not null && swaggerRoutePrefix.Length == 0;

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Shop API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.Configure<PerformanceOptions>(builder.Configuration.GetSection("Performance"));
builder.Services.Configure<BackgroundSyncOptions>(builder.Configuration.GetSection("BackgroundSync"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("ImageProxy");

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/javascript",
        "text/css",
        "text/html",
        "font/woff2"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Add Background Workers
var backgroundSyncEnabled = builder.Configuration.GetValue<bool?>("BackgroundSync:Enabled") ?? true;
if (backgroundSyncEnabled)
{
    builder.Services.AddHostedService<OnlineShop.WebAPI.Workers.MahakSyncWorker>();
    builder.Services.AddHostedService<OnlineShop.WebAPI.Workers.MahakOutgoingSyncWorker>();
    
    // Register Order lock timeout worker
    builder.Services.AddHostedService<OnlineShop.WebAPI.Workers.OrderLockTimeoutWorker>();
}

builder.Services.AddHostedService<OnlineShop.WebAPI.Workers.KeepAliveWorker>();

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
    // Development policy - allow all localhost origins
    options.AddPolicy("DevelopmentCors", policy =>
        policy
            .SetIsOriginAllowed(origin =>
            {
                // Allow all localhost and 127.0.0.1 origins on any port
                if (string.IsNullOrEmpty(origin)) return false;
                
                // Try to parse as URI
                try
                {
                    var uri = new Uri(origin);
                    var host = uri.Host.ToLowerInvariant();
                    
                    // Allow localhost, 127.0.0.1, and private network IPs
                    return host == "localhost" || 
                           host == "127.0.0.1" || 
                           host.StartsWith("192.168.") || 
                           host.StartsWith("172.") ||
                           host.StartsWith("10.") ||
                           // Allow any IP that looks like a local development server
                           (host.Contains("localhost") || host.Contains("127.0.0.1"));
                }
                catch
                {
                    // If parsing fails, allow it in development (for flexibility)
                    return builder.Environment.IsDevelopment();
                }
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("*")
            .SetPreflightMaxAge(TimeSpan.FromHours(1))); // Cache preflight for 1 hour

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

    // Fallback for development - allow all origins (with credentials)
    // TODO: In production, restrict this to specific origins for security
    options.AddPolicy("DefaultCors", policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var configuredSecretLength = JwtSigningKeyProvider.GetConfiguredSecretLength(builder.Configuration);
if (configuredSecretLength < 32)
{
    Log.Warning("JWT secret is not configured or shorter than 32 bytes. Ensure JWT__SECRET environment variable is set in production.");
}
Log.Information("Loaded JWT settings for {Environment}: Issuer='{Issuer}', Audience='{Audience}', SecretLength={SecretLength}",
    builder.Environment.EnvironmentName,
    string.IsNullOrWhiteSpace(issuer) ? "<empty>" : issuer,
    string.IsNullOrWhiteSpace(audience) ? "<empty>" : audience,
    configuredSecretLength);
var key = JwtSigningKeyProvider.CreateKey(builder.Configuration);

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
            IssuerSigningKeyResolver = (_, _, _, _) => new[] { JwtSigningKeyProvider.CreateKey(builder.Configuration) },
            IssuerValidator = (tokenIssuer, _, _) =>
            {
                var validIssuer = builder.Configuration.GetSection("Jwt")["Issuer"];
                if (string.Equals(tokenIssuer, validIssuer, StringComparison.Ordinal))
                {
                    return tokenIssuer;
                }

                throw new SecurityTokenInvalidIssuerException($"Invalid issuer '{tokenIssuer}'.");
            },
            AudienceValidator = (tokenAudiences, _, _) =>
            {
                var validAudience = builder.Configuration.GetSection("Jwt")["Audience"];
                return tokenAudiences.Any(tokenAudience =>
                    string.Equals(tokenAudience, validAudience, StringComparison.Ordinal));
            },
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning(
                    context.Exception,
                    "JWT authentication failed for {Path}: {Message}",
                    context.HttpContext.Request.Path,
                    context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning(
                    "JWT challenge for {Path}. Error={Error}, Description={Description}",
                    context.HttpContext.Request.Path,
                    context.Error,
                    context.ErrorDescription);
                return Task.CompletedTask;
            }
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

var applyMigrationsOnStartup = app.Configuration.GetValue<bool?>("Performance:ApplyMigrationsOnStartup")
                               ?? app.Environment.IsDevelopment();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (applyMigrationsOnStartup &&
        db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        db.Database.Migrate();
    }
    else
    {
        Log.Information("Database migration on startup is disabled.");
    }

    if (shouldSeedDefaults)
    {
        await OnlineShop.Infrastructure.Data.DatabaseSeeder.SeedRolesAsync(scope.ServiceProvider);
    }
    else
    {
        Log.Information("Default user and role seeding is disabled.");
    }
}

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Shop API V1");
        c.RoutePrefix = swaggerAtRoot ? string.Empty : swaggerRoutePrefix;
    });
}


// CORS must be before UseAuthentication and UseAuthorization
// CORS middleware must be called BEFORE UseAuthentication/UseAuthorization
// UseCors automatically handles preflight (OPTIONS) requests
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "AllowFrontend");
app.UseResponseCompression();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<OnlineShop.WebAPI.Middlewares.RequestLoggingMiddleware>();

// Apply request localization
app.UseRequestLocalization();

// Send the root URL straight to the localized storefront instead of showing
// the static redirect placeholder page first.
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "/index.html")
    {
        context.Response.Redirect("/fa/index.html");
        return;
    }

    await next();
});

// Serve default files (index.html) for SPA routing - MUST be before UseStaticFiles
app.UseDefaultFiles();

// Serve static files from wwwroot - MUST be after UseDefaultFiles
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var path = context.Context.Request.Path.Value ?? string.Empty;
        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Context.Response.Headers["Pragma"] = "no-cache";
            context.Context.Response.Headers["Expires"] = "0";
            return;
        }

        if (path.EndsWith("config.runtime.json", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers["Cache-Control"] = "no-cache, must-revalidate";
            context.Context.Response.Headers["Pragma"] = "no-cache";
            context.Context.Response.Headers["Expires"] = "0";
            return;
        }

        if (path.StartsWith("/fa/assets/", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
    }
});

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
