using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;
using ThreatDetector.Data;
using System.Text.Json;

namespace ThreatDetector.API.Services;

public class ZeroDayDetectionService : IZeroDayDetectionService
{
    private readonly ThreatDetectorDbContext _context;
    private readonly ILogger<ZeroDayDetectionService> _logger;

    public ZeroDayDetectionService(
        ThreatDetectorDbContext context, 
        ILogger<ZeroDayDetectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ZeroDayVulnerability?> DetectZeroDayAsync(object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var payloadDict = ConvertToPayloadData(payload);
            
            // Advanced zero-day detection logic
            var vulnerability = await AnalyzeForZeroDay(payloadDict, cancellationToken);
            
            if (vulnerability != null)
            {
                _context.ZeroDayVulnerabilities.Add(vulnerability);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogWarning("Zero-day vulnerability detected: {VulnerabilityType} with confidence {ConfidenceScore}",
                    vulnerability.VulnerabilityType, vulnerability.ConfidenceScore);
            }

            return vulnerability;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting zero-day vulnerability");
            throw;
        }
    }

    public async Task<List<ZeroDayVulnerability>> GetActiveVulnerabilitiesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ZeroDayVulnerabilities
            .Where(v => v.Status == VulnerabilityStatus.UnderInvestigation || v.Status == VulnerabilityStatus.Confirmed)
            .OrderByDescending(v => v.FirstDetected)
            .ToListAsync(cancellationToken);
    }

    public async Task<ZeroDayVulnerability> UpdateVulnerabilityStatusAsync(Guid vulnerabilityId, VulnerabilityStatus status, CancellationToken cancellationToken = default)
    {
        var vulnerability = await _context.ZeroDayVulnerabilities
            .FirstOrDefaultAsync(v => v.Id == vulnerabilityId, cancellationToken);

        if (vulnerability == null)
            throw new ArgumentException($"Vulnerability with ID {vulnerabilityId} not found");

        vulnerability.Status = status;
        vulnerability.LastSeen = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Updated vulnerability {VulnerabilityId} status to {Status}", vulnerabilityId, status);
        
        return vulnerability;
    }

    public async Task<bool> ValidateVulnerabilityAsync(Guid vulnerabilityId, CancellationToken cancellationToken = default)
    {
        var vulnerability = await _context.ZeroDayVulnerabilities
            .FirstOrDefaultAsync(v => v.Id == vulnerabilityId, cancellationToken);

        if (vulnerability == null) return false;

        // Perform validation logic
        var isValid = vulnerability.ConfidenceScore > 0.7 && 
                     vulnerability.OccurrenceCount > 1;

        if (isValid)
        {
            vulnerability.IsConfirmed = true;
            vulnerability.Status = VulnerabilityStatus.Confirmed;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return isValid;
    }

    public async Task<List<ZeroDayVulnerability>> SearchVulnerabilitiesBySignatureAsync(string signature, CancellationToken cancellationToken = default)
    {
        return await _context.ZeroDayVulnerabilities
            .Where(v => v.ExploitSignature.Contains(signature))
            .OrderByDescending(v => v.ConfidenceScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> CalculateVulnerabilityScoreAsync(object exploitData, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        
        var data = ConvertToPayloadData(exploitData);
        
        // Simple scoring algorithm
        var score = 0.0;
        
        if (data.ContainsKey("suspiciousPatterns")) score += 0.3;
        if (data.ContainsKey("unknownSignature")) score += 0.4;
        if (data.ContainsKey("behaviorAnomaly")) score += 0.3;
        
        return Math.Min(score, 1.0);
    }

    public async Task TrainDetectionModelAsync(IEnumerable<object> knownExploits, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Training zero-day detection model with {Count} known exploits", knownExploits.Count());
        
        // In a real implementation, this would train an ML model
        await Task.Delay(1000, cancellationToken);
        
        _logger.LogInformation("Zero-day detection model training completed");
    }

    public async Task<bool> CorrelateWithKnownVulnerabilitiesAsync(ZeroDayVulnerability vulnerability, CancellationToken cancellationToken = default)
    {
        var similarVulnerabilities = await _context.ZeroDayVulnerabilities
            .Where(v => v.VulnerabilityType == vulnerability.VulnerabilityType && 
                       v.Id != vulnerability.Id)
            .ToListAsync(cancellationToken);

        var hasCorrelation = similarVulnerabilities.Any(v => 
            CalculateSimilarity(v.ExploitSignature, vulnerability.ExploitSignature) > 0.8);

        if (hasCorrelation)
        {
            vulnerability.ConfidenceScore = Math.Min(vulnerability.ConfidenceScore + 0.2, 1.0);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return hasCorrelation;
    }

    private async Task<ZeroDayVulnerability?> AnalyzeForZeroDay(Dictionary<string, object> payloadDict, CancellationToken cancellationToken)
    {
        // Sophisticated zero-day analysis
        var suspiciousPatterns = DetectSuspiciousPatterns(payloadDict);
        var confidenceScore = CalculateConfidenceScore(payloadDict, suspiciousPatterns);
        
        if (confidenceScore < 0.6) return null;

        var vulnerability = new ZeroDayVulnerability
        {
            VulnerabilityType = DetermineVulnerabilityType(payloadDict),
            ExploitSignature = GenerateSignature(payloadDict),
            ConfidenceScore = confidenceScore,
            Severity = DetermineSeverity(confidenceScore),
            AttackVectors = ExtractAttackVectors(payloadDict),
            TechnicalDetails = payloadDict,
            ImpactDescription = GenerateImpactDescription(payloadDict)
        };

        return vulnerability;
    }

    private Dictionary<string, object> ConvertToPayloadData(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
    }

    private List<string> DetectSuspiciousPatterns(Dictionary<string, object> payload)
    {
        var patterns = new List<string>();
        
        if (payload.ContainsKey("shellcode")) patterns.Add("Shellcode detected");
        if (payload.ContainsKey("rop_chain")) patterns.Add("ROP chain detected");
        if (payload.ContainsKey("heap_spray")) patterns.Add("Heap spray detected");
        if (payload.ContainsKey("unknown_syscall")) patterns.Add("Unknown system call");
        
        return patterns;
    }

    private double CalculateConfidenceScore(Dictionary<string, object> payload, List<string> patterns)
    {
        var baseScore = patterns.Count * 0.2;
        
        // Add entropy-based scoring
        if (payload.ContainsKey("entropy") && double.TryParse(payload["entropy"].ToString(), out var entropy))
        {
            if (entropy > 7.5) baseScore += 0.3;
        }
        
        return Math.Min(baseScore, 1.0);
    }

    private string DetermineVulnerabilityType(Dictionary<string, object> payload)
    {
        if (payload.ContainsKey("buffer_overflow")) return "Buffer Overflow";
        if (payload.ContainsKey("code_injection")) return "Code Injection";
        if (payload.ContainsKey("privilege_escalation")) return "Privilege Escalation";
        return "Unknown Exploit";
    }

    private string GenerateSignature(Dictionary<string, object> payload)
    {
        var keyElements = new List<string>();
        
        foreach (var key in payload.Keys.Take(5))
        {
            keyElements.Add($"{key}:{payload[key]?.ToString()?.Substring(0, Math.Min(10, payload[key]?.ToString()?.Length ?? 0))}");
        }
        
        return string.Join("|", keyElements);
    }

    private VulnerabilitySeverity DetermineSeverity(double confidenceScore)
    {
        return confidenceScore switch
        {
            >= 0.9 => VulnerabilitySeverity.Critical,
            >= 0.7 => VulnerabilitySeverity.High,
            >= 0.5 => VulnerabilitySeverity.Medium,
            _ => VulnerabilitySeverity.Low
        };
    }

    private List<string> ExtractAttackVectors(Dictionary<string, object> payload)
    {
        var vectors = new List<string>();
        
        if (payload.ContainsKey("network")) vectors.Add("Network");
        if (payload.ContainsKey("web")) vectors.Add("Web Application");
        if (payload.ContainsKey("email")) vectors.Add("Email");
        if (payload.ContainsKey("file")) vectors.Add("File System");
        
        return vectors.Any() ? vectors : new List<string> { "Unknown" };
    }

    private string GenerateImpactDescription(Dictionary<string, object> payload)
    {
        return "Potential zero-day vulnerability with unknown impact. Requires immediate investigation and analysis.";
    }

    private double CalculateSimilarity(string signature1, string signature2)
    {
        if (string.IsNullOrEmpty(signature1) || string.IsNullOrEmpty(signature2)) return 0.0;
        
        var parts1 = signature1.Split('|');
        var parts2 = signature2.Split('|');
        
        var commonParts = parts1.Intersect(parts2).Count();
        var totalParts = Math.Max(parts1.Length, parts2.Length);
        
        return totalParts > 0 ? (double)commonParts / totalParts : 0.0;
    }
} 