using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using ThreatDetector.Core.Models;

namespace ThreatDetector.SDK;

/// <summary>
/// Main SDK client for AI Threat Detection integration
/// </summary>
public interface IThreatDetectorClient
{
    /// <summary>
    /// Analyze data for potential threats
    /// </summary>
    Task<ThreatDetectionResult> AnalyzeThreatAsync(object data, string? source = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze user behavior for anomalies
    /// </summary>
    Task<UserBehaviorResult> AnalyzeUserBehaviorAsync(string userId, object eventData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detect zero-day vulnerabilities
    /// </summary>
    Task<ZeroDayDetectionResult> DetectZeroDayAsync(object payload, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get active threats
    /// </summary>
    Task<List<ThreatDetectionResult>> GetActiveThreatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submit training data to improve detection models
    /// </summary>
    Task<bool> SubmitTrainingDataAsync(IEnumerable<object> trainingData, string modelType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate threat score for given data
    /// </summary>
    Task<double> GetThreatScoreAsync(object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mitigate a detected threat
    /// </summary>
    Task<bool> MitigateThreatAsync(Guid threatId, string action, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start real-time threat monitoring
    /// </summary>
    Task StartRealTimeMonitoringAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stop real-time threat monitoring
    /// </summary>
    Task StopRealTimeMonitoringAsync();
    
    /// <summary>
    /// Events for threat detection notifications
    /// </summary>
    event ThreatDetectedEventHandler? ThreatDetected;
    event UserBehaviorAnomalyEventHandler? UserBehaviorAnomalyDetected;
    event ZeroDayDetectedEventHandler? ZeroDayDetected;
    event SystemAlertEventHandler? SystemAlert;
}

/// <summary>
/// Implementation of the Threat Detector SDK client
/// </summary>
public class ThreatDetectorClient : IThreatDetectorClient, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ThreatDetectorOptions _options;
    private readonly ILogger<ThreatDetectorClient> _logger;
    private HubConnection? _hubConnection;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    public event ThreatDetectedEventHandler? ThreatDetected;
    public event UserBehaviorAnomalyEventHandler? UserBehaviorAnomalyDetected;
    public event ZeroDayDetectedEventHandler? ZeroDayDetected;
    public event SystemAlertEventHandler? SystemAlert;

    public ThreatDetectorClient(
        HttpClient httpClient,
        IOptions<ThreatDetectorOptions> options,
        ILogger<ThreatDetectorClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
        
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }
        
        _httpClient.DefaultRequestHeaders.Add("X-Application-Id", _options.ApplicationId);
    }

    public async Task<ThreatDetectionResult> AnalyzeThreatAsync(object data, string? source = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                data,
                source = source ?? _options.ApplicationId,
                timestamp = DateTime.UtcNow,
                applicationId = _options.ApplicationId
            };

            var response = await _httpClient.PostAsJsonAsync("api/threatdetection/analyze", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var threatEvent = await response.Content.ReadFromJsonAsync<ThreatEvent>(cancellationToken);
            
            var result = MapThreatEventToResult(threatEvent);
            
            // Trigger event if threat detected and above threshold
            if (result.IsThreat && result.ThreatScore >= _options.ThreatThreshold)
            {
                await OnThreatDetected(result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing threat data");
            throw new ThreatDetectorException("Failed to analyze threat data", ex);
        }
    }

    public async Task<UserBehaviorResult> AnalyzeUserBehaviorAsync(string userId, object eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableUserBehaviorMonitoring)
            {
                return new UserBehaviorResult { UserId = userId, IsAnomalous = false };
            }

            var response = await _httpClient.PostAsJsonAsync($"api/userbehavior/analyze/{userId}", eventData, cancellationToken);
            response.EnsureSuccessStatusCode();

            var behaviorEvent = await response.Content.ReadFromJsonAsync<UserBehaviorEvent>(cancellationToken);
            
            var result = MapUserBehaviorEventToResult(behaviorEvent);
            
            // Trigger event if anomaly detected
            if (result.IsAnomalous)
            {
                await OnUserBehaviorAnomalyDetected(result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user behavior for user {UserId}", userId);
            throw new ThreatDetectorException($"Failed to analyze user behavior for user {userId}", ex);
        }
    }

    public async Task<ZeroDayDetectionResult> DetectZeroDayAsync(object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableZeroDayDetection)
            {
                return new ZeroDayDetectionResult { IsZeroDay = false };
            }

            var response = await _httpClient.PostAsJsonAsync("api/zeroday/detect", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (responseContent.Contains("No zero-day vulnerability detected"))
            {
                return new ZeroDayDetectionResult { IsZeroDay = false };
            }

            var vulnerability = await response.Content.ReadFromJsonAsync<ZeroDayVulnerability>(cancellationToken);
            
            var result = MapZeroDayVulnerabilityToResult(vulnerability);
            
            // Trigger event if zero-day detected
            if (result.IsZeroDay)
            {
                await OnZeroDayDetected(result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting zero-day vulnerability");
            throw new ThreatDetectorException("Failed to detect zero-day vulnerability", ex);
        }
    }

    public async Task<List<ThreatDetectionResult>> GetActiveThreatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/threatdetection/active", cancellationToken);
            response.EnsureSuccessStatusCode();

            var threats = await response.Content.ReadFromJsonAsync<List<ThreatEvent>>(cancellationToken);
            
            return threats?.Select(MapThreatEventToResult).ToList() ?? new List<ThreatDetectionResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active threats");
            throw new ThreatDetectorException("Failed to retrieve active threats", ex);
        }
    }

    public async Task<bool> SubmitTrainingDataAsync(IEnumerable<object> trainingData, string modelType, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = trainingData.ToList();
            var batches = data.Chunk(_options.BatchSize);

            foreach (var batch in batches)
            {
                var response = await _httpClient.PostAsJsonAsync($"api/learning/retrain/{modelType}", batch, cancellationToken);
                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Successfully submitted {Count} training samples for model {ModelType}", data.Count, modelType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting training data for model {ModelType}", modelType);
            return false;
        }
    }

    public async Task<double> GetThreatScoreAsync(object data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/threatdetection/score", data, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>(cancellationToken);
            return result?["score"] ?? 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating threat score");
            throw new ThreatDetectorException("Failed to calculate threat score", ex);
        }
    }

    public async Task<bool> MitigateThreatAsync(Guid threatId, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.AutoMitigation)
            {
                _logger.LogWarning("Auto-mitigation is disabled. Threat {ThreatId} requires manual intervention", threatId);
                return false;
            }

            var response = await _httpClient.PostAsJsonAsync($"api/threatdetection/{threatId}/mitigate", action, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>(cancellationToken);
            return result?["success"] ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mitigating threat {ThreatId}", threatId);
            return false;
        }
    }

    public async Task StartRealTimeMonitoringAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.EnableRealTimeNotifications)
        {
            return;
        }

        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_options.ApiBaseUrl}/hubs/threatdetection")
                .WithAutomaticReconnect()
                .Build();

            ConfigureHubEvents();
            
            await _hubConnection.StartAsync(cancellationToken);
            await _hubConnection.SendAsync("JoinGroup", _options.ApplicationId, cancellationToken);
            
            _logger.LogInformation("Real-time threat monitoring started for application {ApplicationId}", _options.ApplicationId);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task StopRealTimeMonitoringAsync()
    {
        await _connectionSemaphore.WaitAsync();
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _logger.LogInformation("Real-time threat monitoring stopped");
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private void ConfigureHubEvents()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<string>("ThreatDetected", async (threatJson) =>
        {
            try
            {
                var threat = JsonSerializer.Deserialize<ThreatEvent>(threatJson);
                if (threat != null)
                {
                    var result = MapThreatEventToResult(threat);
                    await OnThreatDetected(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing real-time threat notification");
            }
        });

        _hubConnection.On<string>("UserBehaviorAnomaly", async (behaviorJson) =>
        {
            try
            {
                var behavior = JsonSerializer.Deserialize<UserBehaviorEvent>(behaviorJson);
                if (behavior != null)
                {
                    var result = MapUserBehaviorEventToResult(behavior);
                    await OnUserBehaviorAnomalyDetected(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing real-time behavior anomaly notification");
            }
        });

        _hubConnection.On<string>("ZeroDayDetected", async (zeroDayJson) =>
        {
            try
            {
                var zeroDay = JsonSerializer.Deserialize<ZeroDayVulnerability>(zeroDayJson);
                if (zeroDay != null)
                {
                    var result = MapZeroDayVulnerabilityToResult(zeroDay);
                    await OnZeroDayDetected(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing real-time zero-day notification");
            }
        });

        _hubConnection.On<string, string, string>("SystemAlert", async (alertType, message, contextJson) =>
        {
            try
            {
                var context = JsonSerializer.Deserialize<Dictionary<string, object>>(contextJson) ?? new();
                await OnSystemAlert(alertType, message, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing real-time system alert");
            }
        });
    }

    private async Task OnThreatDetected(ThreatDetectionResult threat)
    {
        if (ThreatDetected != null)
        {
            await ThreatDetected(threat);
        }
    }

    private async Task OnUserBehaviorAnomalyDetected(UserBehaviorResult behavior)
    {
        if (UserBehaviorAnomalyDetected != null)
        {
            await UserBehaviorAnomalyDetected(behavior);
        }
    }

    private async Task OnZeroDayDetected(ZeroDayDetectionResult zeroDay)
    {
        if (ZeroDayDetected != null)
        {
            await ZeroDayDetected(zeroDay);
        }
    }

    private async Task OnSystemAlert(string alertType, string message, Dictionary<string, object> context)
    {
        if (SystemAlert != null)
        {
            await SystemAlert(alertType, message, context);
        }
    }

    private static ThreatDetectionResult MapThreatEventToResult(ThreatEvent? threat)
    {
        if (threat == null)
        {
            return new ThreatDetectionResult { IsThreat = false };
        }

        return new ThreatDetectionResult
        {
            ThreatId = threat.Id,
            IsThreat = threat.Status != ThreatStatus.Resolved,
            ThreatScore = threat.ConfidenceScore,
            ThreatType = threat.ThreatType,
            Description = threat.Description,
            DetectedAt = threat.CreatedAt,
            Source = threat.Source,
            Target = threat.Target,
            Metadata = threat.Metadata ?? new(),
            Severity = threat.Severity.ToString()
        };
    }

    private static UserBehaviorResult MapUserBehaviorEventToResult(UserBehaviorEvent? behavior)
    {
        if (behavior == null)
        {
            return new UserBehaviorResult { IsAnomalous = false };
        }

        return new UserBehaviorResult
        {
            UserId = behavior.UserId,
            IsAnomalous = behavior.IsAnomaly,
            AnomalyScore = behavior.AnomalyScore,
            RiskLevel = behavior.RiskLevel.ToString(),
            AnomalyReasons = behavior.AnomalyReasons ?? new(),
            AnalyzedAt = behavior.Timestamp,
            EventData = behavior.EventData ?? new()
        };
    }

    private static ZeroDayDetectionResult MapZeroDayVulnerabilityToResult(ZeroDayVulnerability? vulnerability)
    {
        if (vulnerability == null)
        {
            return new ZeroDayDetectionResult { IsZeroDay = false };
        }

        return new ZeroDayDetectionResult
        {
            IsZeroDay = true,
            ConfidenceScore = vulnerability.ConfidenceScore,
            VulnerabilityType = vulnerability.VulnerabilityType,
            AffectedSystem = vulnerability.AffectedSystem,
            Severity = vulnerability.Severity.ToString(),
            Description = vulnerability.Description,
            DetectedAt = vulnerability.DetectedAt,
            Indicators = vulnerability.Indicators ?? new(),
            TechnicalDetails = vulnerability.TechnicalDetails ?? new()
        };
    }

    public async ValueTask DisposeAsync()
    {
        await StopRealTimeMonitoringAsync();
        _connectionSemaphore.Dispose();
        _httpClient.Dispose();
    }
}

/// <summary>
/// Custom exception for Threat Detector SDK errors
/// </summary>
public class ThreatDetectorException : Exception
{
    public ThreatDetectorException(string message) : base(message) { }
    public ThreatDetectorException(string message, Exception innerException) : base(message, innerException) { }
}
