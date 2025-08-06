using ThreatDetector.Core.Models;

namespace ThreatDetector.Core.Interfaces;

public interface IUserBehaviorAnalysisService
{
    Task<UserBehaviorEvent> AnalyzeBehaviorAsync(string userId, object eventData, CancellationToken cancellationToken = default);
    
    Task<List<UserBehaviorEvent>> GetAnomalousBehaviorAsync(string? userId = null, DateTime? since = null, CancellationToken cancellationToken = default);
    
    Task<double> CalculateAnomalyScoreAsync(string userId, object eventData, CancellationToken cancellationToken = default);
    
    Task UpdateUserBaselineAsync(string userId, CancellationToken cancellationToken = default);
    
    Task<List<UserBehaviorEvent>> GetUserBehaviorHistoryAsync(string userId, DateTime? since = null, int? limit = null, CancellationToken cancellationToken = default);
    
    Task<BehaviorRiskLevel> AssessRiskLevelAsync(string userId, CancellationToken cancellationToken = default);
    
    Task TrainBehaviorModelAsync(string userId, IEnumerable<UserBehaviorEvent> historicalData, CancellationToken cancellationToken = default);
    
    Task<bool> IsHighRiskSessionAsync(string sessionId, CancellationToken cancellationToken = default);
} 