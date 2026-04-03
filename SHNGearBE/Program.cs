using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Configurations;
using SHNGearBE.Extensions;
using SHNGearBE.Helpers.Authorization;
using SHNGearBE.Middlewares;
using BackgroundLogService;
using SHNGearMailService.Extensions;
using System.Linq;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure form data and request body size limits for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 15 * 1024 * 1024; // 15MB
    options.ValueLengthLimit = 15 * 1024 * 1024;
    options.MultipartHeadersLengthLimit = 2 * 1024 * 1024; // 2MB for headers
});

// Configure CORS
builder.Services.AddCors(options =>
{
    var configuredOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
        ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray()
        ?? [];

    // Keep local defaults for developer experience when no config is provided.
    if (configuredOrigins.Length == 0)
    {
        configuredOrigins = ["http://localhost:3000", "http://localhost:5173"];
    }

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(configuredOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

// Rate limiting policy for public endpoints like payment webhooks.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PayPalWebhook", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Register Database, Repositories, and Services
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddCloudinarySettings(builder.Configuration);
builder.Services.AddPayPalSettings(builder.Configuration);
builder.Services.AddMailService(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JwtSettings configuration is missing");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization with Permission-based policies
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

//Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SHNGear API", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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

// Log Service - Typed injection
builder.Services.AddTypedLogService(builder.Configuration);

// Uncomment when ready to use Redis
// builder.Services.AddRedisCache(builder.Configuration);

// Uncomment when ready to use RabbitMQ
// builder.Services.AddRabbitMQ(builder.Configuration);

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
