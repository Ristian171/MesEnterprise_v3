using EFCore.NamingConventions;
using MesEnterprise.Data;
using MesEnterprise.Services;
using MesEnterprise.Services.BackgroundServices;
using MesEnterprise.Infrastructure;
using MesEnterprise.Endpoints;
using MesEnterprise.Endpoints.Enterprise;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Start of Recommended Serilog Configuration ---
// Configure Serilog directly on the host builder. This ensures it has
// access to the full application configuration (appsettings, env vars, etc.).
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
// --- End of Recommended Serilog Configuration ---

try
{
    // Detect if running under EF Core tools to prevent runtime services from starting.
    // This is the most reliable way to detect the EF Core tools context.
    // The tools consistently pass the path to `ef.dll` as an argument.
    var isEfTool = args.Any(a => a.EndsWith("ef.dll", StringComparison.OrdinalIgnoreCase));

    var configuration = builder.Configuration;

    // JSON configuration
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { 
            Title = "MES Enterprise API", 
            Version = "v1",
            Description = "Manufacturing Execution System - Enterprise Edition"
        });
        options.AddSecurityDefinition("Bearer", new()
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please enter JWT token: Bearer {token}",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    if (!isEfTool)
    {
        // Database configuration for runtime. This is skipped during design-time
        // because the DesignTimeDbContextFactory is used instead.
        var connectionString = Environment.GetEnvironmentVariable("MES_CONN_STRING")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured. Set MES_CONN_STRING environment variable or add to appsettings.json.");
        }
        builder.Services.AddDbContext<MesDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                   .UseSnakeCaseNamingConvention()
        );
    }

    // Services
    builder.Services.AddScoped<PasswordService>();
    builder.Services.AddScoped<TokenService>();

    if (!isEfTool)
    {
        // Background Services should not run during design-time operations like migrations.
        // Configure a shared initial delay for all background services.
        builder.Services.Configure<BackgroundServiceOptions>(options =>
        {
            options.InitialDelay = TimeSpan.FromSeconds(10);
        });

        builder.Services.AddHostedService<JustificationCheckService>();
        builder.Services.AddHostedService<AutoBackupService>();
        builder.Services.AddHostedService<PmSchedulerService>();
        builder.Services.AddHostedService<AlertScannerService>();
        builder.Services.AddHostedService<EquipmentHourTrackingService>();
        builder.Services.AddHostedService<TokenCleanupService>();
        builder.Services.AddHostedService<ExportWorkerService>();
    }

    // JWT Authentication
    var jwtKey = Environment.GetEnvironmentVariable("MES_JWT_KEY")
        ?? configuration["JwtSettings:Key"]
        ?? throw new InvalidOperationException("JWT Key not configured. Set MES_JWT_KEY environment variable.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"] ?? "MES_Enterprise",
                ValidAudience = configuration["JwtSettings:Audience"] ?? "MES_Enterprise",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("TechOrAdmin", policy => policy.RequireRole("Admin", "Technician", "InginerMentenanta"));
        options.AddPolicy("OperatorOrHigher", policy => policy.RequireRole("Admin", "Technician", "Operator", "InginerMentenanta", "PlantManager", "TeamLeader", "Quality", "Warehouse", "Planner"));
    });

    // CORS configuration
    var allowedOrigins = configuration.GetValue<string>("CorsSettings:AllowedOrigins")?.Split(',') 
        ?? new[] { "http://localhost:5000" };
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            b => b.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("Content-Disposition"));
    });

    var app = builder.Build();

    if (!isEfTool)
    {
        // Initialize database (migrations + seed data), but not during design-time.
        await DatabaseInitializer.InitializeDatabase(app.Services);
    }

    // Middleware pipeline
    app.UseCors("AllowSpecificOrigins");

    // HSTS in production
    if (app.Environment.IsProduction())
    {
        app.UseHsts();
    }

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "login.html" } });
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    // Module gate middleware (after auth)
    app.UseMiddleware<ModuleGateMiddleware>();

    // Map Core API endpoints
    app.MapAuthApi();
    app.MapConfigApi();
    app.MapOperatorApi();
    app.MapProductionApi();
    app.MapInterventiiApi();
    app.MapChangeoverApi();
    app.MapPublicApi();
    app.MapExportApi();
    
    // Map Enterprise endpoints
    app.MapPlanningEndpoints();
    app.MapAdminEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException) // Don't log HostAbortedException, which is common during EF tooling.
{
    // Use the host's logger for consistency, if available.
    // Fallback to a temporary logger if the host failed to build.
    Log.Logger = Log.Logger ?? new LoggerConfiguration().WriteTo.Console().CreateLogger();
    Log.Fatal(ex, "Unhandled exception during application startup");
}
finally
{
    Log.CloseAndFlush();
}
