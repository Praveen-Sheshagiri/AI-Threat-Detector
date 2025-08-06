using ThreatDetector.Core.Models;

namespace ThreatDetector.Core.Interfaces;

public interface IZeroDayDetectionService
{
    Task<ZeroDayVulnerability?> DetectZeroDayAsync(object payload, CancellationToken cancellationToken = default);
    
    Task<List<ZeroDayVulnerability>> GetActiveVulnerabilitiesAsync(CancellationToken cancellationToken = default);
    
    Task<ZeroDayVulnerability> UpdateVulnerabilityStatusAsync(Guid vulnerabilityId, VulnerabilityStatus status, CancellationToken cancellationToken = default);
    
    Task<bool> ValidateVulnerabilityAsync(Guid vulnerabilityId, CancellationToken cancellationToken = default);
    
    Task<List<ZeroDayVulnerability>> SearchVulnerabilitiesBySignatureAsync(string signature, CancellationToken cancellationToken = default);
    
    Task<double> CalculateVulnerabilityScoreAsync(object exploitData, CancellationToken cancellationToken = default);
    
    Task TrainDetectionModelAsync(IEnumerable<object> knownExploits, CancellationToken cancellationToken = default);
    
    Task <bool> CorrelateWithKnownVulnerabilitiesAsync(ZeroDayVulnerability vulnerability, CancellationToken cancellationToken = default);
} 