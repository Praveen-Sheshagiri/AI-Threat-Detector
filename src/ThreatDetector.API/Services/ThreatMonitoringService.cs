using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Services;

public class ThreatMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ThreatMonitoringService> _logger;
    private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(30);

    public ThreatMonitoringService(
        IServiceProvider serviceProvider,
        ILogger<ThreatMonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Threat Monitoring Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformMonitoringCycle(stoppingToken);
                await Task.Delay(_monitoringInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in threat monitoring cycle");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Threat Monitoring Service stopped");
    }

    private async Task PerformMonitoringCycle(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var threatDetectionService = scope.ServiceProvider.GetRequiredService<IThreatDetectionService>();
        var userBehaviorService = scope.ServiceProvider.GetRequiredService<IUserBehaviorAnalysisService>();
        var zeroDayService = scope.ServiceProvider.GetRequiredService<IZeroDayDetectionService>();
        var alertingService = scope.ServiceProvider.GetRequiredService<IAlertingService>();

        // Monitor active threats
        await MonitorActiveThreats(threatDetectionService, alertingService, cancellationToken);
        
        // Monitor user behavior anomalies
        await MonitorUserBehaviorAnomalies(userBehaviorService, alertingService, cancellationToken);
        
        // Monitor zero-day vulnerabilities
        await MonitorZeroDayVulnerabilities(zeroDayService, alertingService, cancellationToken);
        
        // Perform system health checks
        await PerformSystemHealthCheck(alertingService, cancellationToken);

        _logger.LogDebug("Monitoring cycle completed successfully");
    }

    private async Task MonitorActiveThreats(
        IThreatDetectionService threatService, 
        IAlertingService alertingService, 
        CancellationToken cancellationToken)
    {
        try
        {
            var activeThreats = await threatService.GetActiveThreatsAsync(cancellationToken);
            
            foreach (var threat in activeThreats.Where(t => t.Severity >= ThreatSeverity.High))
            {
                // Check if we need to escalate or create alerts for high-severity threats
                if (ShouldCreateAlert(threat))
                {
                    var alert = new SecurityAlert
                    {
                        Title = $"High Severity Threat Detected: {threat.ThreatType}",
                        Message = $"Threat detected from {threat.Source} targeting {threat.Target}. Confidence: {threat.ConfidenceScore:P2}",
                        Severity = MapThreatSeverityToAlertSeverity(threat.Severity),
                        Type = AlertType.ThreatDetection,
                        Source = "ThreatMonitoringService",
                        RelatedThreatId = threat.Id.ToString(),
                        AffectedSystems = new List<string> { threat.Target },
                        RecommendedActions = GenerateRecommendedActions(threat),
                        Context = new Dictionary<string, object>
                        {
                            ["threatType"] = threat.ThreatType,
                            ["confidence"] = threat.ConfidenceScore,
                            ["isZeroDay"] = threat.IsZeroDay
                        }
                    };

                    await alertingService.CreateAlertAsync(alert, cancellationToken);
                }
            }

            if (activeThreats.Any())
            {
                _logger.LogInformation("Monitoring {ThreatCount} active threats", activeThreats.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring active threats");
        }
    }

    private async Task MonitorUserBehaviorAnomalies(
        IUserBehaviorAnalysisService behaviorService, 
        IAlertingService alertingService, 
        CancellationToken cancellationToken)
    {
        try
        {
            var anomalies = await behaviorService.GetAnomalousBehaviorAsync(
                since: DateTime.UtcNow.AddMinutes(-5), 
                cancellationToken: cancellationToken);

            foreach (var anomaly in anomalies.Where(a => a.RiskLevel >= BehaviorRiskLevel.High))
            {
                var alert = new SecurityAlert
                {
                    Title = $"User Behavior Anomaly Detected: {anomaly.UserId}",
                    Message = $"Anomalous behavior detected for user {anomaly.UserId}. Risk Level: {anomaly.RiskLevel}, Anomaly Score: {anomaly.AnomalyScore:P2}",
                    Severity = MapBehaviorRiskToAlertSeverity(anomaly.RiskLevel),
                    Type = AlertType.UserBehaviorAnomaly,
                    Source = "ThreatMonitoringService",
                    AffectedSystems = new List<string> { "User Authentication System" },
                    RecommendedActions = GenerateUserBehaviorRecommendations(anomaly),
                    Context = new Dictionary<string, object>
                    {
                        ["userId"] = anomaly.UserId,
                        ["anomalyScore"] = anomaly.AnomalyScore,
                        ["riskLevel"] = anomaly.RiskLevel.ToString(),
                        ["anomalyReasons"] = anomaly.AnomalyReasons
                    }
                };

                await alertingService.CreateAlertAsync(alert, cancellationToken);
            }

            if (anomalies.Any())
            {
                _logger.LogInformation("Monitoring {AnomalyCount} user behavior anomalies", anomalies.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring user behavior anomalies");
        }
    }

    private async Task MonitorZeroDayVulnerabilities(
        IZeroDayDetectionService zeroDayService, 
        IAlertingService alertingService, 
        CancellationToken cancellationToken)
    {
        try
        {
            var vulnerabilities = await zeroDayService.GetActiveVulnerabilitiesAsync(cancellationToken);
            
            foreach (var vulnerability in vulnerabilities.Where(v => v.Severity >= VulnerabilitySeverity.High))
            {
                var alert = new SecurityAlert
                {
                    Title = $"Zero-Day Vulnerability Detected: {vulnerability.VulnerabilityType}",
                    Message = $"Potential zero-day vulnerability detected in {vulnerability.AffectedSystem}. Confidence: {vulnerability.ConfidenceScore:P2}",
                    Severity = MapVulnerabilitySeverityToAlertSeverity(vulnerability.Severity),
                    Type = AlertType.ZeroDayVulnerability,
                    Source = "ThreatMonitoringService",
                    RelatedVulnerabilityId = vulnerability.Id.ToString(),
                    AffectedSystems = new List<string> { vulnerability.AffectedSystem },
                    RecommendedActions = GenerateVulnerabilityRecommendations(vulnerability),
                    Context = new Dictionary<string, object>
                    {
                        ["vulnerabilityType"] = vulnerability.VulnerabilityType,
                        ["affectedSystem"] = vulnerability.AffectedSystem,
                        ["confidence"] = vulnerability.ConfidenceScore,
                        ["attackVectors"] = vulnerability.AttackVectors
                    }
                };

                await alertingService.CreateAlertAsync(alert, cancellationToken);
            }

            if (vulnerabilities.Any())
            {
                _logger.LogInformation("Monitoring {VulnerabilityCount} zero-day vulnerabilities", vulnerabilities.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring zero-day vulnerabilities");
        }
    }

    private async Task PerformSystemHealthCheck(IAlertingService alertingService, CancellationToken cancellationToken)
    {
        try
        {
            // Simulate system health metrics
            var cpuUsage = Random.Shared.NextDouble() * 100;
            var memoryUsage = Random.Shared.NextDouble() * 100;
            var diskUsage = Random.Shared.NextDouble() * 100;

            // Check for system resource alerts
            if (cpuUsage > 90)
            {
                var alert = new SecurityAlert
                {
                    Title = "High CPU Usage Detected",
                    Message = $"System CPU usage is at {cpuUsage:F1}%, which may impact threat detection performance",
                    Severity = AlertSeverity.Medium,
                    Type = AlertType.SystemMalfunction,
                    Source = "ThreatMonitoringService",
                    AffectedSystems = new List<string> { "Threat Detection System" },
                    RecommendedActions = new List<string> 
                    { 
                        "Monitor system performance", 
                        "Consider scaling resources",
                        "Review running processes"
                    }
                };

                await alertingService.CreateAlertAsync(alert, cancellationToken);
            }

            _logger.LogDebug("System health check completed. CPU: {CPU:F1}%, Memory: {Memory:F1}%, Disk: {Disk:F1}%",
                cpuUsage, memoryUsage, diskUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing system health check");
        }
    }

    private bool ShouldCreateAlert(ThreatEvent threat)
    {
        // Logic to determine if an alert should be created
        // This could check if an alert was already created recently for similar threats
        return threat.Severity >= ThreatSeverity.High && 
               (DateTime.UtcNow - threat.DetectedAt) < TimeSpan.FromMinutes(5);
    }

    private AlertSeverity MapThreatSeverityToAlertSeverity(ThreatSeverity severity)
    {
        return severity switch
        {
            ThreatSeverity.Critical => AlertSeverity.Critical,
            ThreatSeverity.High => AlertSeverity.High,
            ThreatSeverity.Medium => AlertSeverity.Medium,
            ThreatSeverity.Low => AlertSeverity.Low,
            _ => AlertSeverity.Info
        };
    }

    private AlertSeverity MapBehaviorRiskToAlertSeverity(BehaviorRiskLevel riskLevel)
    {
        return riskLevel switch
        {
            BehaviorRiskLevel.Critical => AlertSeverity.Critical,
            BehaviorRiskLevel.High => AlertSeverity.High,
            BehaviorRiskLevel.Medium => AlertSeverity.Medium,
            BehaviorRiskLevel.Low => AlertSeverity.Low,
            _ => AlertSeverity.Info
        };
    }

    private AlertSeverity MapVulnerabilitySeverityToAlertSeverity(VulnerabilitySeverity severity)
    {
        return severity switch
        {
            VulnerabilitySeverity.Critical => AlertSeverity.Critical,
            VulnerabilitySeverity.High => AlertSeverity.High,
            VulnerabilitySeverity.Medium => AlertSeverity.Medium,
            VulnerabilitySeverity.Low => AlertSeverity.Low,
            VulnerabilitySeverity.Informational => AlertSeverity.Info,
            _ => AlertSeverity.Info
        };
    }

    private List<string> GenerateRecommendedActions(ThreatEvent threat)
    {
        var actions = new List<string>();
        
        switch (threat.ThreatType.ToLower())
        {
            case "malware":
                actions.AddRange(new[] { "Quarantine affected systems", "Run full antivirus scan", "Update security patches" });
                break;
            case "ddos":
                actions.AddRange(new[] { "Enable DDoS protection", "Block source IPs", "Scale resources" });
                break;
            default:
                actions.AddRange(new[] { "Investigate immediately", "Monitor affected systems", "Consider isolation" });
                break;
        }
        
        return actions;
    }

    private List<string> GenerateUserBehaviorRecommendations(UserBehaviorEvent anomaly)
    {
        return new List<string>
        {
            "Review user account activity",
            "Verify user identity",
            "Consider multi-factor authentication",
            "Monitor user sessions closely"
        };
    }

    private List<string> GenerateVulnerabilityRecommendations(ZeroDayVulnerability vulnerability)
    {
        return new List<string>
        {
            "Isolate affected systems",
            "Apply emergency patches if available",
            "Monitor for exploit attempts",
            "Implement additional security controls"
        };
    }
} 