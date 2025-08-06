using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Logging;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;
using ThreatDetector.ML.Models;
using System.Text.Json;

namespace ThreatDetector.ML.Services;

public class UserBehaviorAnalysisMLService : IUserBehaviorAnalysisService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<UserBehaviorAnalysisMLService> _logger;
    private ITransformer? _model;
    private PredictionEngine<UserBehaviorInput, ThreatPredictionOutput>? _predictionEngine;
    private readonly string _modelPath = "Models/user-behavior-model.zip";
    private readonly Dictionary<string, UserProfile> _userProfiles = new();

    public UserBehaviorAnalysisMLService(ILogger<UserBehaviorAnalysisMLService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        LoadOrCreateModel();
    }

    public async Task<UserBehaviorEvent> AnalyzeBehaviorAsync(string userId, object eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = ConvertToMLInput(userId, eventData);
            var anomalyScore = await CalculateAnomalyScoreAsync(userId, eventData, cancellationToken);
            
            var behaviorEvent = new UserBehaviorEvent
            {
                UserId = userId,
                EventType = ExtractEventType(eventData),
                IpAddress = ExtractIpAddress(eventData),
                UserAgent = ExtractUserAgent(eventData),
                Location = ExtractLocation(eventData),
                EventData = ConvertToEventData(eventData),
                AnomalyScore = anomalyScore,
                IsAnomaly = anomalyScore > 0.7,
                AnomalyReasons = GenerateAnomalyReasons(input, anomalyScore),
                SessionId = ExtractSessionId(eventData),
                DeviceFingerprint = ExtractDeviceFingerprint(eventData),
                RiskLevel = DetermineRiskLevel(anomalyScore)
            };

            _logger.LogInformation("User behavior analyzed for {UserId}. Anomaly Score: {AnomalyScore}, Is Anomaly: {IsAnomaly}",
                userId, anomalyScore, behaviorEvent.IsAnomaly);

            return behaviorEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user behavior for {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserBehaviorEvent>> GetAnomalousBehaviorAsync(string? userId = null, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        // This would typically query a database
        await Task.Delay(50, cancellationToken);
        return new List<UserBehaviorEvent>();
    }

    public async Task<double> CalculateAnomalyScoreAsync(string userId, object eventData, CancellationToken cancellationToken = default)
    {
        var input = ConvertToMLInput(userId, eventData);
        var userProfile = GetOrCreateUserProfile(userId);
        
        // Calculate anomaly based on user's historical behavior
        var timeAnomalyScore = CalculateTimeAnomalyScore(input, userProfile);
        var locationAnomalyScore = CalculateLocationAnomalyScore(input, userProfile);
        var behaviorAnomalyScore = CalculateBehaviorAnomalyScore(input, userProfile);
        
        // Use ML model if available
        var mlScore = 0.0;
        if (_predictionEngine != null)
        {
            var prediction = _predictionEngine.Predict(input);
            mlScore = prediction.Probability;
        }

        // Combine scores
        var combinedScore = (timeAnomalyScore * 0.3 + locationAnomalyScore * 0.3 + behaviorAnomalyScore * 0.2 + mlScore * 0.2);
        
        await Task.Delay(10, cancellationToken);
        return Math.Min(combinedScore, 1.0);
    }

    public async Task UpdateUserBaselineAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userProfile = GetOrCreateUserProfile(userId);
        
        // This would typically analyze recent user behavior and update the baseline
        _logger.LogInformation("Updating baseline for user {UserId}", userId);
        
        await Task.Delay(100, cancellationToken);
    }

    public async Task<List<UserBehaviorEvent>> GetUserBehaviorHistoryAsync(string userId, DateTime? since = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        // This would typically query a database
        await Task.Delay(50, cancellationToken);
        return new List<UserBehaviorEvent>();
    }

    public async Task<BehaviorRiskLevel> AssessRiskLevelAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userProfile = GetOrCreateUserProfile(userId);
        
        // Assess risk based on recent behavior patterns
        var recentAnomalies = userProfile.RecentAnomalies.Count(a => a > DateTime.UtcNow.AddDays(-7));
        
        await Task.Delay(25, cancellationToken);
        
        return recentAnomalies switch
        {
            >= 10 => BehaviorRiskLevel.Critical,
            >= 5 => BehaviorRiskLevel.High,
            >= 2 => BehaviorRiskLevel.Medium,
            _ => BehaviorRiskLevel.Low
        };
    }

    public async Task TrainBehaviorModelAsync(string userId, IEnumerable<UserBehaviorEvent> historicalData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Training behavior model for user {UserId} with {Count} historical events", 
            userId, historicalData.Count());

        var mlInputs = historicalData.Select(e => ConvertToMLInput(userId, e.EventData)).ToList();
        
        if (mlInputs.Any())
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(mlInputs);
            
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("IpAddressFeatures", nameof(UserBehaviorInput.IpAddress))
                .Append(_mlContext.Transforms.Text.FeaturizeText("LocationFeatures", nameof(UserBehaviorInput.Location)))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "IpAddressFeatures", "LocationFeatures",
                    nameof(UserBehaviorInput.LoginTime),
                    nameof(UserBehaviorInput.SessionDuration),
                    nameof(UserBehaviorInput.PageViewCount),
                    nameof(UserBehaviorInput.ClickRate),
                    nameof(UserBehaviorInput.TypingSpeed),
                    nameof(UserBehaviorInput.BandwidthUsage),
                    nameof(UserBehaviorInput.AccessFrequency),
                    nameof(UserBehaviorInput.DataDownloaded),
                    nameof(UserBehaviorInput.DataUploaded),
                    nameof(UserBehaviorInput.FailedLoginAttempts)))
                .Append(_mlContext.AnomalyDetection.Trainers.RandomizedPca());

            _model = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<UserBehaviorInput, ThreatPredictionOutput>(_model);
            
            // Save model
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        }

        await Task.CompletedTask;
    }

    public async Task<bool> IsHighRiskSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        // This would typically analyze session data from database
        await Task.Delay(25, cancellationToken);
        return false; // Simplified for demo
    }

    private void LoadOrCreateModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<UserBehaviorInput, ThreatPredictionOutput>(_model);
                _logger.LogInformation("Loaded existing user behavior model");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user behavior model");
        }
    }

    private UserBehaviorInput ConvertToMLInput(string userId, object eventData)
    {
        var json = JsonSerializer.Serialize(eventData);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();

        return new UserBehaviorInput
        {
            UserId = userId,
            LoginTime = Convert.ToSingle(dict.GetValueOrDefault("loginTime", 0)),
            IpAddress = dict.GetValueOrDefault("ipAddress")?.ToString(),
            Location = dict.GetValueOrDefault("location")?.ToString(),
            SessionDuration = Convert.ToSingle(dict.GetValueOrDefault("sessionDuration", 0)),
            PageViewCount = Convert.ToSingle(dict.GetValueOrDefault("pageViewCount", 0)),
            ClickRate = Convert.ToSingle(dict.GetValueOrDefault("clickRate", 0)),
            TypingSpeed = Convert.ToSingle(dict.GetValueOrDefault("typingSpeed", 0)),
            MouseMovementPattern = Convert.ToSingle(dict.GetValueOrDefault("mouseMovementPattern", 0)),
            DeviceType = dict.GetValueOrDefault("deviceType")?.ToString(),
            OperatingSystem = dict.GetValueOrDefault("operatingSystem")?.ToString(),
            Browser = dict.GetValueOrDefault("browser")?.ToString(),
            ScreenResolution = Convert.ToSingle(dict.GetValueOrDefault("screenResolution", 0)),
            BandwidthUsage = Convert.ToSingle(dict.GetValueOrDefault("bandwidthUsage", 0)),
            AccessFrequency = Convert.ToSingle(dict.GetValueOrDefault("accessFrequency", 0)),
            AccessPattern = dict.GetValueOrDefault("accessPattern")?.ToString(),
            DataDownloaded = Convert.ToSingle(dict.GetValueOrDefault("dataDownloaded", 0)),
            DataUploaded = Convert.ToSingle(dict.GetValueOrDefault("dataUploaded", 0)),
            FailedLoginAttempts = Convert.ToSingle(dict.GetValueOrDefault("failedLoginAttempts", 0))
        };
    }

    private UserProfile GetOrCreateUserProfile(string userId)
    {
        if (!_userProfiles.ContainsKey(userId))
        {
            _userProfiles[userId] = new UserProfile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
        return _userProfiles[userId];
    }

    private double CalculateTimeAnomalyScore(UserBehaviorInput input, UserProfile profile)
    {
        // Simple time-based anomaly detection
        var currentHour = DateTime.UtcNow.Hour;
        var typicalHours = profile.TypicalAccessHours;
        
        if (!typicalHours.Any()) return 0.0;
        
        return typicalHours.Contains(currentHour) ? 0.0 : 0.8;
    }

    private double CalculateLocationAnomalyScore(UserBehaviorInput input, UserProfile profile)
    {
        if (string.IsNullOrEmpty(input.Location)) return 0.0;
        
        return profile.KnownLocations.Contains(input.Location) ? 0.0 : 0.9;
    }

    private double CalculateBehaviorAnomalyScore(UserBehaviorInput input, UserProfile profile)
    {
        // Simple behavioral analysis
        var score = 0.0;
        
        if (input.FailedLoginAttempts > 3) score += 0.5;
        if (input.AccessFrequency > profile.AverageAccessFrequency * 3) score += 0.3;
        if (input.SessionDuration > profile.AverageSessionDuration * 5) score += 0.2;
        
        return Math.Min(score, 1.0);
    }

    private BehaviorRiskLevel DetermineRiskLevel(double anomalyScore)
    {
        return anomalyScore switch
        {
            >= 0.9 => BehaviorRiskLevel.Critical,
            >= 0.7 => BehaviorRiskLevel.High,
            >= 0.5 => BehaviorRiskLevel.Medium,
            _ => BehaviorRiskLevel.Low
        };
    }

    private List<string> GenerateAnomalyReasons(UserBehaviorInput input, double anomalyScore)
    {
        var reasons = new List<string>();
        
        if (input.FailedLoginAttempts > 3)
            reasons.Add("Multiple failed login attempts");
        if (input.AccessFrequency > 100)
            reasons.Add("Unusually high access frequency");
        if (anomalyScore > 0.8)
            reasons.Add("Behavioral pattern significantly different from baseline");
            
        return reasons;
    }

    // Helper methods for extracting data from event
    private string ExtractEventType(object eventData) => "UserAction"; // Simplified
    private string ExtractIpAddress(object eventData) => "127.0.0.1"; // Simplified
    private string ExtractUserAgent(object eventData) => "Unknown"; // Simplified
    private string ExtractLocation(object eventData) => "Unknown"; // Simplified
    private string ExtractSessionId(object eventData) => Guid.NewGuid().ToString(); // Simplified
    private string ExtractDeviceFingerprint(object eventData) => "Unknown"; // Simplified
    
    private Dictionary<string, object> ConvertToEventData(object eventData)
    {
        var json = JsonSerializer.Serialize(eventData);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
    }
}

public class UserProfile
{
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public HashSet<string> KnownLocations { get; set; } = new();
    public HashSet<int> TypicalAccessHours { get; set; } = new();
    public float AverageAccessFrequency { get; set; }
    public float AverageSessionDuration { get; set; }
    public List<DateTime> RecentAnomalies { get; set; } = new();
} 