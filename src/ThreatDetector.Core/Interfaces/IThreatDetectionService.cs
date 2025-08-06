using ThreatDetector.Core.Models;

namespace ThreatDetector.Core.Interfaces;

public interface IThreatDetectionService
{
    Task<ThreatEvent> AnalyzeAsync(object data, CancellationToken cancellationToken = default);
    
    Task<List<ThreatEvent>> GetActiveThreatsAsync(CancellationToken cancellationToken = default);
    
    Task<bool> MitigateThreatAsync(Guid threatId, string action, CancellationToken cancellationToken = default);
    
    Task<ThreatEvent> UpdateThreatStatusAsync(Guid threatId, ThreatStatus status, CancellationToken cancellationToken = default);
    
    Task<List<ThreatEvent>> GetThreatsByTypeAsync(string threatType, DateTime? since = null, CancellationToken cancellationToken = default);
    
    Task<double> GetThreatScoreAsync(object data, CancellationToken cancellationToken = default);
    
    Task TrainModelAsync(IEnumerable<object> trainingData, CancellationToken cancellationToken = default);
    
    Task<bool> IsZeroDayThreatAsync(ThreatEvent threat, CancellationToken cancellationToken = default);
} 