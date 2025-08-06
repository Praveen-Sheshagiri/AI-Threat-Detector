using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThreatDetector.API.Hubs;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;
using ThreatDetector.Data;

namespace ThreatDetector.API.Services;

public class AlertingService : IAlertingService
{
    private readonly ThreatDetectorDbContext _context;
    private readonly IHubContext<ThreatDetectionHub> _hubContext;
    private readonly ILogger<AlertingService> _logger;

    public AlertingService(
        ThreatDetectorDbContext context,
        IHubContext<ThreatDetectionHub> hubContext,
        ILogger<AlertingService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<SecurityAlert> CreateAlertAsync(SecurityAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            alert.CreatedAt = DateTime.UtcNow;
            alert.Status = AlertStatus.Active;

            _context.SecurityAlerts.Add(alert);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created security alert: {Title} with severity {Severity}", 
                alert.Title, alert.Severity);

            // Send real-time notification
            await _hubContext.Clients.All.SendAsync("SecurityAlert", alert, cancellationToken);

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            throw;
        }
    }

    public async Task<bool> SendAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await _context.SecurityAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

            if (alert == null)
            {
                _logger.LogWarning("Alert {AlertId} not found", alertId);
                return false;
            }

            // Send via multiple channels based on severity
            await SendEmailAlert(alert, cancellationToken);
            
            if (alert.Severity >= AlertSeverity.High)
            {
                await SendSmsAlert(alert, cancellationToken);
            }

            if (alert.Severity == AlertSeverity.Critical)
            {
                await SendPushNotification(alert, cancellationToken);
            }

            // Send real-time notification
            await _hubContext.Clients.All.SendAsync("SecurityAlert", alert, cancellationToken);

            _logger.LogInformation("Sent alert {AlertId} via multiple channels", alertId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<SecurityAlert> AcknowledgeAlertAsync(Guid alertId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        var alert = await _context.SecurityAlerts
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert == null)
            throw new ArgumentException($"Alert with ID {alertId} not found");

        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = acknowledgedBy;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alert {AlertId} acknowledged by {User}", alertId, acknowledgedBy);

        // Notify all clients about the acknowledgment
        await _hubContext.Clients.All.SendAsync("AlertAcknowledged", alert, cancellationToken);

        return alert;
    }

    public async Task<List<SecurityAlert>> GetActiveAlertsAsync(AlertSeverity? minSeverity = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityAlerts
            .Where(a => a.Status == AlertStatus.Active);

        if (minSeverity.HasValue)
        {
            query = query.Where(a => a.Severity >= minSeverity.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SecurityAlert> UpdateAlertStatusAsync(Guid alertId, AlertStatus status, CancellationToken cancellationToken = default)
    {
        var alert = await _context.SecurityAlerts
            .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

        if (alert == null)
            throw new ArgumentException($"Alert with ID {alertId} not found");

        var previousStatus = alert.Status;
        alert.Status = status;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated alert {AlertId} status from {PreviousStatus} to {NewStatus}", 
            alertId, previousStatus, status);

        // Notify all clients about the status change
        await _hubContext.Clients.All.SendAsync("AlertStatusChanged", alert, cancellationToken);

        return alert;
    }

    public async Task<bool> DismissAlertAsync(Guid alertId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await _context.SecurityAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

            if (alert == null)
            {
                _logger.LogWarning("Alert {AlertId} not found", alertId);
                return false;
            }

            alert.Status = AlertStatus.Dismissed;
            alert.Context["dismissalReason"] = reason;
            alert.Context["dismissedAt"] = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Dismissed alert {AlertId} with reason: {Reason}", alertId, reason);

            // Notify all clients about the dismissal
            await _hubContext.Clients.All.SendAsync("AlertDismissed", alert, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<List<SecurityAlert>> GetAlertsByTypeAsync(AlertType type, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityAlerts
            .Where(a => a.Type == type);

        if (since.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= since.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EscalateAlertAsync(Guid alertId, AlertSeverity newSeverity, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await _context.SecurityAlerts
                .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

            if (alert == null)
            {
                _logger.LogWarning("Alert {AlertId} not found", alertId);
                return false;
            }

            var previousSeverity = alert.Severity;
            alert.Severity = newSeverity;
            alert.Priority = (int)newSeverity;
            alert.Context["escalatedAt"] = DateTime.UtcNow;
            alert.Context["previousSeverity"] = previousSeverity.ToString();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Escalated alert {AlertId} from {PreviousSeverity} to {NewSeverity}", 
                alertId, previousSeverity, newSeverity);

            // Send additional notifications for escalated alerts
            if (newSeverity == AlertSeverity.Critical)
            {
                await SendEmergencyNotification(alert, cancellationToken);
            }

            // Notify all clients about the escalation
            await _hubContext.Clients.All.SendAsync("AlertEscalated", alert, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating alert {AlertId}", alertId);
            return false;
        }
    }

    public async Task<Dictionary<AlertSeverity, int>> GetAlertStatisticsAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityAlerts.AsQueryable();

        if (since.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= since.Value);
        }

        var statistics = await query
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Severity, x => x.Count, cancellationToken);

        // Ensure all severity levels are represented
        foreach (AlertSeverity severity in Enum.GetValues<AlertSeverity>())
        {
            if (!statistics.ContainsKey(severity))
            {
                statistics[severity] = 0;
            }
        }

        return statistics;
    }

    private async Task SendEmailAlert(SecurityAlert alert, CancellationToken cancellationToken)
    {
        // Simulate email sending
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Email alert sent for {AlertId}", alert.Id);
    }

    private async Task SendSmsAlert(SecurityAlert alert, CancellationToken cancellationToken)
    {
        // Simulate SMS sending
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("SMS alert sent for {AlertId}", alert.Id);
    }

    private async Task SendPushNotification(SecurityAlert alert, CancellationToken cancellationToken)
    {
        // Simulate push notification
        await Task.Delay(50, cancellationToken);
        _logger.LogInformation("Push notification sent for {AlertId}", alert.Id);
    }

    private async Task SendEmergencyNotification(SecurityAlert alert, CancellationToken cancellationToken)
    {
        // Simulate emergency notification (could be Slack, Teams, PagerDuty, etc.)
        await Task.Delay(50, cancellationToken);
        _logger.LogCritical("Emergency notification sent for critical alert {AlertId}: {Title}", 
            alert.Id, alert.Title);
    }
} 