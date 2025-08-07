using Microsoft.AspNetCore.Mvc;
using ThreatDetector.SDK;

namespace ThreatDetector.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThreatDemoController : ControllerBase
{
    private readonly IThreatDetectorClient _threatDetectorClient;
    private readonly ILogger<ThreatDemoController> _logger;

    public ThreatDemoController(
        IThreatDetectorClient threatDetectorClient,
        ILogger<ThreatDemoController> logger)
    {
        _threatDetectorClient = threatDetectorClient;
        _logger = logger;
    }

    /// <summary>
    /// Simulate various threat scenarios for demonstration
    /// </summary>
    [HttpPost("simulate/{threatType}")]
    public async Task<ActionResult> SimulateThreat(string threatType, [FromBody] object? payload = null)
    {
        try
        {
            var result = threatType.ToLower() switch
            {
                "sqli" => await SimulateSqlInjection(),
                "xss" => await SimulateXssAttack(),
                "ddos" => await SimulateDdosAttack(),
                "bruteforce" => await SimulateBruteForceAttack(),
                "malware" => await SimulateMalwareDetection(),
                "zeroday" => await SimulateZeroDayVulnerability(),
                "suspicious" => await SimulateSuspiciousBehavior(),
                "anomaly" => await SimulateUserAnomalyDetection(),
                _ => throw new ArgumentException($"Unknown threat type: {threatType}")
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating threat {ThreatType}", threatType);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all active threats detected in the demo
    /// </summary>
    [HttpGet("active-threats")]
    public async Task<ActionResult> GetActiveThreats()
    {
        try
        {
            var threats = await _threatDetectorClient.GetActiveThreatsAsync();
            return Ok(new { threats = threats, count = threats.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active threats");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Calculate threat score for custom payload
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult> AnalyzeThreat([FromBody] object payload)
    {
        try
        {
            var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "CustomAnalysis");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing custom threat");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Submit training data to improve threat detection
    /// </summary>
    [HttpPost("train")]
    public async Task<ActionResult> SubmitTrainingData([FromBody] TrainingDataRequest request)
    {
        try
        {
            var success = await _threatDetectorClient.SubmitTrainingDataAsync(
                request.TrainingData, request.ModelType);
            
            return Ok(new { success, message = success ? "Training data submitted successfully" : "Failed to submit training data" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting training data");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task<object> SimulateSqlInjection()
    {
        var maliciousQuery = new
        {
            query = "'; DROP TABLE Users; --",
            targetTable = "Users",
            ipAddress = "192.168.1.100",
            userAgent = "Mozilla/5.0 (Malicious Bot)",
            timestamp = DateTime.UtcNow,
            attackType = "SQLInjection",
            payload = "1' OR '1'='1",
            encodedPayload = false
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(maliciousQuery, "SQLInjectionSimulation");
        
        _logger.LogWarning("🚨 SQL Injection attack simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "SQL Injection",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated SQL injection attack with malicious query",
            mitigationActions = new[] { "Sanitize input", "Use parameterized queries", "Implement WAF rules" },
            result
        };
    }

    private async Task<object> SimulateXssAttack()
    {
        var xssPayload = new
        {
            userInput = "<script>alert('XSS Attack!');</script>",
            targetField = "comment",
            ipAddress = "203.0.113.42",
            userAgent = "Mozilla/5.0 (Attacker Browser)",
            timestamp = DateTime.UtcNow,
            attackType = "CrossSiteScripting",
            payloadType = "ScriptTag",
            encodedPayload = false
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(xssPayload, "XSSSimulation");
        
        _logger.LogWarning("🚨 XSS attack simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "Cross-Site Scripting (XSS)",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated XSS attack with malicious script injection",
            mitigationActions = new[] { "Input validation", "Output encoding", "Content Security Policy" },
            result
        };
    }

    private async Task<object> SimulateDdosAttack()
    {
        var ddosPattern = new
        {
            requestsPerSecond = 1000,
            sourceIps = new[] { "198.51.100.1", "198.51.100.2", "198.51.100.3" },
            targetEndpoint = "/api/products",
            timestamp = DateTime.UtcNow,
            attackType = "DDoS",
            requestSize = 1024,
            userAgents = new[] { "Bot1", "Bot2", "Bot3" },
            duration = TimeSpan.FromMinutes(5).TotalSeconds
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(ddosPattern, "DDoSSimulation");
        
        _logger.LogWarning("🚨 DDoS attack simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "Distributed Denial of Service (DDoS)",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated DDoS attack with high request volume",
            mitigationActions = new[] { "Rate limiting", "IP blocking", "CDN protection", "Load balancing" },
            result
        };
    }

    private async Task<object> SimulateBruteForceAttack()
    {
        var bruteForceData = new
        {
            username = "admin",
            failedAttempts = 15,
            timeWindow = TimeSpan.FromMinutes(5).TotalMinutes,
            sourceIp = "192.0.2.50",
            attackType = "BruteForce",
            timestamp = DateTime.UtcNow,
            userAgent = "Hydra/8.6",
            targetService = "Login"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(bruteForceData, "BruteForceSimulation");
        
        _logger.LogWarning("🚨 Brute force attack simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "Brute Force Attack",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated brute force login attempt",
            mitigationActions = new[] { "Account lockout", "CAPTCHA", "2FA", "IP blocking" },
            result
        };
    }

    private async Task<object> SimulateMalwareDetection()
    {
        var suspiciousFile = new
        {
            fileName = "update.exe",
            fileSize = 2048576,
            fileHash = "d41d8cd98f00b204e9800998ecf8427e",
            uploadSource = "email_attachment",
            timestamp = DateTime.UtcNow,
            threatType = "Malware",
            entropy = 7.8,
            packedExecutable = true,
            suspiciousStrings = new[] { "keylogger", "backdoor", "rootkit" }
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(suspiciousFile, "MalwareSimulation");
        
        _logger.LogWarning("🚨 Malware detection simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "Malware",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated malware file upload detection",
            mitigationActions = new[] { "Quarantine file", "Scan system", "Update antivirus", "Block source" },
            result
        };
    }

    private async Task<object> SimulateZeroDayVulnerability()
    {
        var zeroDayPayload = new
        {
            vulnerabilityType = "Buffer Overflow",
            targetSystem = "Web Application Framework",
            exploitCode = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            timestamp = DateTime.UtcNow,
            sourceIp = "10.0.0.1",
            userAgent = "Custom Exploit Tool v1.0",
            payload = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x31, 0xc0, 0x50, 0x68 },
            memoryCorruption = true,
            returnAddressOverwrite = true
        };

        var result = await _threatDetectorClient.DetectZeroDayAsync(zeroDayPayload);
        
        _logger.LogError("💀 Zero-day vulnerability simulated. Score: {ConfidenceScore}", result.ConfidenceScore);
        
        return new
        {
            threatType = "Zero-Day Vulnerability",
            detected = result.IsZeroDay,
            confidenceScore = result.ConfidenceScore,
            description = "Simulated zero-day exploit attempt",
            mitigationActions = new[] { "Isolate system", "Apply emergency patches", "Monitor closely", "Incident response" },
            result
        };
    }

    private async Task<object> SimulateSuspiciousBehavior()
    {
        var suspiciousActivity = new
        {
            activityType = "Unusual Data Access",
            dataVolume = 10000000, // 10MB
            accessTime = DateTime.UtcNow.AddHours(-2), // 2 AM
            userRole = "StandardUser",
            accessedTables = new[] { "CustomerData", "FinancialRecords", "PersonalInfo" },
            timestamp = DateTime.UtcNow,
            ipAddress = "172.16.0.100",
            userAgent = "Custom Script v1.0",
            sessionDuration = TimeSpan.FromHours(8).TotalMinutes
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(suspiciousActivity, "SuspiciousBehaviorSimulation");
        
        _logger.LogWarning("🔍 Suspicious behavior simulated. Score: {ThreatScore}", result.ThreatScore);
        
        return new
        {
            threatType = "Suspicious Data Access",
            detected = result.IsThreat,
            threatScore = result.ThreatScore,
            description = "Simulated insider threat with unusual data access patterns",
            mitigationActions = new[] { "Investigate user activity", "Restrict access", "Monitor closely", "Review permissions" },
            result
        };
    }

    private async Task<object> SimulateUserAnomalyDetection()
    {
        var anomalousUserData = new
        {
            eventType = "LoginFromNewLocation",
            ipAddress = "185.220.101.1", // Tor exit node
            userAgent = "Tor Browser",
            loginTime = DateTime.UtcNow.AddHours(-12), // Unusual time
            location = "Anonymous Proxy",
            previousLocation = "New York, USA",
            deviceFingerprint = "unknown_device_123",
            sessionBehavior = new
            {
                rapidClicks = 150,
                unusualNavigation = true,
                dataDownloaded = 50000000, // 50MB
                multipleFailedActions = 10
            }
        };

        var result = await _threatDetectorClient.AnalyzeUserBehaviorAsync("demo_user_123", anomalousUserData);
        
        _logger.LogWarning("🔍 User behavior anomaly simulated. Score: {AnomalyScore}", result.AnomalyScore);
        
        return new
        {
            threatType = "User Behavior Anomaly",
            detected = result.IsAnomalous,
            anomalyScore = result.AnomalyScore,
            riskLevel = result.RiskLevel,
            description = "Simulated anomalous user behavior detection",
            mitigationActions = new[] { "Verify user identity", "Require 2FA", "Monitor session", "Limit access" },
            result
        };
    }
}

public class TrainingDataRequest
{
    public List<object> TrainingData { get; set; } = new();
    public string ModelType { get; set; } = "ThreatDetection";
}
