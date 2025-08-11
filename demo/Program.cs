using Microsoft.EntityFrameworkCore;
using Serilog;
using ThreatDetector.SDK.Extensions;
using ThreatDetector.SDK;
using ThreatDetector.Demo.Services;
using ThreatDetector.Demo.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/demo-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AI Threat Detector Demo API",
        Version = "v1",
        Description = "Demo application showcasing AI Threat Detection integration"
    });
});

// Add Entity Framework with In-Memory database for demo
builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemory("DemoDatabase"));

// Add Threat Detector SDK
builder.Services.AddThreatDetector(options =>
{
    options.ApiBaseUrl = builder.Configuration["ThreatDetector:ApiBaseUrl"] ?? "https://localhost:7001";
    options.ApplicationId = "ThreatDetector-Demo";
    options.EnableRealTimeNotifications = true;
    options.EnableUserBehaviorMonitoring = true;
    options.EnableZeroDayDetection = true;
    options.ThreatThreshold = 0.6; // Lower threshold for demo
    options.AutoMitigation = false; // Manual mitigation for demo
});

// Add demo services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IThreatDemoService, ThreatDemoService>();

// Add session for user tracking
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add CORS for demo purposes
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSession();

// Add Threat Detector middleware
app.UseThreatDetector();

app.UseAuthorization();
app.MapControllers();

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await SeedDemoData(context);
}

// Start threat monitoring
using (var scope = app.Services.CreateScope())
{
    var threatDetectorClient = scope.ServiceProvider.GetRequiredService<IThreatDetectorClient>();
    
    // Configure event handlers
    threatDetectorClient.ThreatDetected += async (threat) =>
    {
        Log.Warning("🚨 Threat Detected: {ThreatType} with score {ThreatScore}", threat.ThreatType, threat.ThreatScore);
    };
    
    threatDetectorClient.UserBehaviorAnomalyDetected += async (behavior) =>
    {
        Log.Warning("🔍 User Behavior Anomaly: User {UserId} with score {AnomalyScore}", behavior.UserId, behavior.AnomalyScore);
    };
    
    threatDetectorClient.ZeroDayDetected += async (zeroDay) =>
    {
        Log.Error("💀 Zero-Day Vulnerability Detected: {VulnerabilityType} in {AffectedSystem}", zeroDay.VulnerabilityType, zeroDay.AffectedSystem);
    };
    
    // Start real-time monitoring
    _ = Task.Run(async () =>
    {
        try
        {
            await threatDetectorClient.StartRealTimeMonitoringAsync();
            Log.Information("🛡️ Real-time threat monitoring started");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start real-time threat monitoring");
        }
    });
}

Log.Information("🚀 Demo application started. Navigate to /swagger to explore the API");
app.Run();

static async Task SeedDemoData(DemoDbContext context)
{
    if (!context.Products.Any())
    {
        var products = new[]
        {
            new Product { Name = "Laptop Pro", Price = 1299.99m, Category = "Electronics", Description = "High-performance laptop" },
            new Product { Name = "Smartphone X", Price = 899.99m, Category = "Electronics", Description = "Latest smartphone model" },
            new Product { Name = "Wireless Headphones", Price = 199.99m, Category = "Audio", Description = "Premium wireless headphones" },
            new Product { Name = "Gaming Mouse", Price = 79.99m, Category = "Gaming", Description = "Professional gaming mouse" },
            new Product { Name = "4K Monitor", Price = 499.99m, Category = "Electronics", Description = "Ultra HD 4K monitor" }
        };
        
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    if (!context.Users.Any())
    {
        var users = new[]
        {
            new User { Username = "john_doe", Email = "john@example.com", Role = "User", IsActive = true },
            new User { Username = "jane_admin", Email = "jane@example.com", Role = "Admin", IsActive = true },
            new User { Username = "bob_user", Email = "bob@example.com", Role = "User", IsActive = true },
            new User { Username = "alice_mod", Email = "alice@example.com", Role = "Moderator", IsActive = true }
        };
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
