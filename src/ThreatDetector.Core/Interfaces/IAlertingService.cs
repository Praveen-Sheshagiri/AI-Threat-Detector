using ThreatDetector.Core.Models;

namespace ThreatDetector.Core.Interfaces;

public interface IAlertingService
{
    Task<SecurityAlert> CreateAlertAsync(SecurityAlert alert, CancellationToken cancellationToken = default);
    
    Task<bool> SendAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    
    Task<SecurityAlert> AcknowledgeAlertAsync(Guid alertId, string acknowledgedBy, CancellationToken cancellationToken = default);
    
    Task<List<SecurityAlert>> GetActiveAlertsAsync(AlertSeverity? minSeverity = null, CancellationToken cancellationToken = default);
    
    Task<SecurityAlert> UpdateAlertStatusAsync(Guid alertId, AlertStatus status, CancellationToken cancellationToken = default);
    
    Task<bool> DismissAlertAsync(Guid alertId, string reason, CancellationToken cancellationToken = default);
    
    Task<List<SecurityAlert>> GetAlertsByTypeAsync(AlertType type, DateTime? since = null, CancellationToken cancellationToken = default);
    
    Task<bool> EscalateAlertAsync(Guid alertId, AlertSeverity newSeverity, CancellationToken cancellationToken = default);
    
    Task<Dictionary<AlertSeverity, int>> GetAlertStatisticsAsync(DateTime? since = null, CancellationToken cancellationToken = default);
} 