using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace ThreatDetector.SDK;

/// <summary>
/// Configuration options for the Threat Detector SDK
/// </summary>
public class ThreatDetectorOptions
{
    public const string SectionName = "ThreatDetector";
    
    /// <summary>
    /// Base URL of the Threat Detector API
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://localhost:7001";
    
    /// <summary>
    /// API key for authentication
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Application identifier for tracking
    /// </summary>
    public string ApplicationId { get; set; } = "Default";
    
    /// <summary>
    /// Enable real-time notifications
    /// </summary>
    public bool EnableRealTimeNotifications { get; set; } = true;
    
    /// <summary>
    /// Timeout for API requests in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Minimum threat score to trigger alerts (0.0 to 1.0)
    /// </summary>
    public double ThreatThreshold { get; set; } = 0.7;
    
    /// <summary>
    /// Enable automatic threat mitigation
    /// </summary>
    public bool AutoMitigation { get; set; } = false;
    
    /// <summary>
    /// Batch size for bulk operations
    /// </summary>
    public int BatchSize { get; set; } = 100;
    
    /// <summary>
    /// Enable user behavior monitoring
    /// </summary>
    public bool EnableUserBehaviorMonitoring { get; set; } = true;
    
    /// <summary>
    /// Enable zero-day detection
    /// </summary>
    public bool EnableZeroDayDetection { get; set; } = true;
}

/// <summary>
/// Represents a threat detection result
/// </summary>
public class ThreatDetectionResult
{
    public bool IsThreat { get; set; }
    public double ThreatScore { get; set; }
    public string ThreatType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public string Severity { get; set; } = "Low";
    public Guid ThreatId { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Represents user behavior analysis result
/// </summary>
public class UserBehaviorResult
{
    public string UserId { get; set; } = string.Empty;
    public bool IsAnomalous { get; set; }
    public double AnomalyScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<string> AnomalyReasons { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> EventData { get; set; } = new();
}

/// <summary>
/// Represents a zero-day vulnerability detection result
/// </summary>
public class ZeroDayDetectionResult
{
    public bool IsZeroDay { get; set; }
    public double ConfidenceScore { get; set; }
    public string VulnerabilityType { get; set; } = string.Empty;
    public string AffectedSystem { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low";
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public List<string> Indicators { get; set; } = new();
    public Dictionary<string, object> TechnicalDetails { get; set; } = new();
}

/// <summary>
/// Event handler delegates for threat detection events
/// </summary>
public delegate Task ThreatDetectedEventHandler(ThreatDetectionResult threat);
public delegate Task UserBehaviorAnomalyEventHandler(UserBehaviorResult behavior);
public delegate Task ZeroDayDetectedEventHandler(ZeroDayDetectionResult zeroDay);
public delegate Task SystemAlertEventHandler(string alertType, string message, Dictionary<string, object> context);
