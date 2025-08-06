using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.ML.Services;
using ThreatDetector.Data;
using ThreatDetector.API.Hubs;
using ThreatDetector.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/threat-detector-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Threat Detector API",
        Version = "v1",
        Description = "Advanced AI-powered threat detection and security monitoring system",
        Contact = new OpenApiContact
        {
            Name = "AI Threat Detector Team"
        }
    });
});

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure Entity Framework
builder.Services.AddDbContext<ThreatDetectorDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=ThreatDetectorDb;Trusted_Connection=true;MultipleActiveResultSets=true";
    options.UseSqlServer(connectionString);
});

// Register ML Services
builder.Services.AddSingleton<IThreatDetectionService, ThreatDetectionMLService>();
builder.Services.AddSingleton<IUserBehaviorAnalysisService, UserBehaviorAnalysisMLService>();
builder.Services.AddScoped<IZeroDayDetectionService, ZeroDayDetectionService>();
builder.Services.AddScoped<IContinuousLearningService, ContinuousLearningService>();
builder.Services.AddScoped<IAlertingService, AlertingService>();

// Register background services
builder.Services.AddHostedService<ThreatMonitoringService>();
builder.Services.AddHostedService<ModelRetrainingService>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContext<ThreatDetectorDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Threat Detector API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHub<ThreatDetectionHub>("/threatHub");
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ThreatDetectorDbContext>();
    context.Database.EnsureCreated();
}

app.Run(); 