using SHNGearBE.Extensions;
using SHNGearBE.Middlewares;
using BackgroundLogService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register Database, Repositories, and Services
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Log Service - Typed injection
builder.Services.AddTypedLogService(builder.Configuration);

// Uncomment when ready to use Redis
// builder.Services.AddRedisCache(builder.Configuration);

// Uncomment when ready to use RabbitMQ
// builder.Services.AddRabbitMQ(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
