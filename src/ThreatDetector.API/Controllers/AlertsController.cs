using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertingService _alertingService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(
        IAlertingService alertingService,
        ILogger<AlertsController> logger)
    {
        _alertingService = alertingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new security alert
    /// </summary>
    /// <param name="alert">Alert to create</param>
    /// <returns>Created alert</returns>
    [HttpPost]
    public async Task<ActionResult<SecurityAlert>> CreateAlertAsync([FromBody] SecurityAlert alert)
    {
        try
        {
            var createdAlert = await _alertingService.CreateAlertAsync(alert);
            return CreatedAtAction(nameof(GetAlertAsync), new { id = createdAlert.Id }, createdAlert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            return StatusCode(500, new { error = "Internal server error creating alert" });
        }
    }

    /// <summary>
    /// Get alert by ID
    /// </summary>
    /// <param name="id">Alert ID</param>
    /// <returns>Security alert</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SecurityAlert>> GetAlertAsync(Guid id)
    {
        try
        {
            var alerts = await _alertingService.GetActiveAlertsAsync();
            var alert = alerts.FirstOrDefault(a => a.Id == id);
            
            if (alert == null)
            {
                return NotFound(new { error = "Alert not found" });
            }
            
            return Ok(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert {AlertId}", id);
            return StatusCode(500, new { error = "Internal server error retrieving alert" });
        }
    }

    /// <summary>
    /// Get all active alerts
    /// </summary>
    /// <param name="minSeverity">Minimum severity filter</param>
    /// <returns>List of active alerts</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<SecurityAlert>>> GetActiveAlertsAsync(
        [FromQuery] AlertSeverity? minSeverity = null)
    {
        try
        {
            var alerts = await _alertingService.GetActiveAlertsAsync(minSeverity);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts");
            return StatusCode(500, new { error = "Internal server error retrieving alerts" });
        }
    }

    /// <summary>
    /// Get alerts by type
    /// </summary>
    /// <param name="type">Alert type</param>
    /// <param name="since">Optional date filter</param>
    /// <returns>List of alerts</returns>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<List<SecurityAlert>>> GetAlertsByTypeAsync(
        AlertType type,
        [FromQuery] DateTime? since = null)
    {
        try
        {
            var alerts = await _alertingService.GetAlertsByTypeAsync(type, since);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts by type {AlertType}", type);
            return StatusCode(500, new { error = "Internal server error retrieving alerts" });
        }
    }

    /// <summary>
    /// Send an alert via configured channels
    /// </summary>
    /// <param name="alertId">Alert ID to send</param>
    /// <returns>Success status</returns>
    [HttpPost("{alertId}/send")]
    public async Task<ActionResult> SendAlertAsync(Guid alertId)
    {
        try
        {
            var success = await _alertingService.SendAlertAsync(alertId);
            if (success)
            {
                return Ok(new { message = "Alert sent successfully" });
            }
            return NotFound(new { error = "Alert not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Internal server error sending alert" });
        }
    }

    /// <summary>
    /// Acknowledge an alert
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="acknowledgedBy">User acknowledging the alert</param>
    /// <returns>Updated alert</returns>
    [HttpPost("{alertId}/acknowledge")]
    public async Task<ActionResult<SecurityAlert>> AcknowledgeAlertAsync(
        Guid alertId,
        [FromBody] string acknowledgedBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(acknowledgedBy))
            {
                return BadRequest(new { error = "AcknowledgedBy parameter is required" });
            }

            var acknowledgedAlert = await _alertingService.AcknowledgeAlertAsync(alertId, acknowledgedBy);
            return Ok(acknowledgedAlert);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Internal server error acknowledging alert" });
        }
    }

    /// <summary>
    /// Update alert status
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated alert</returns>
    [HttpPut("{alertId}/status")]
    public async Task<ActionResult<SecurityAlert>> UpdateAlertStatusAsync(
        Guid alertId,
        [FromBody] AlertStatus status)
    {
        try
        {
            var updatedAlert = await _alertingService.UpdateAlertStatusAsync(alertId, status);
            return Ok(updatedAlert);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert status for {AlertId}", alertId);
            return StatusCode(500, new { error = "Internal server error updating alert status" });
        }
    }

    /// <summary>
    /// Dismiss an alert
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="reason">Dismissal reason</param>
    /// <returns>Success status</returns>
    [HttpPost("{alertId}/dismiss")]
    public async Task<ActionResult> DismissAlertAsync(
        Guid alertId,
        [FromBody] string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest(new { error = "Reason parameter is required" });
            }

            var success = await _alertingService.DismissAlertAsync(alertId, reason);
            if (success)
            {
                return Ok(new { message = "Alert dismissed successfully" });
            }
            return NotFound(new { error = "Alert not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Internal server error dismissing alert" });
        }
    }

    /// <summary>
    /// Escalate an alert
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="newSeverity">New severity level</param>
    /// <returns>Success status</returns>
    [HttpPost("{alertId}/escalate")]
    public async Task<ActionResult> EscalateAlertAsync(
        Guid alertId,
        [FromBody] AlertSeverity newSeverity)
    {
        try
        {
            var success = await _alertingService.EscalateAlertAsync(alertId, newSeverity);
            if (success)
            {
                return Ok(new { message = "Alert escalated successfully", newSeverity });
            }
            return NotFound(new { error = "Alert not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating alert {AlertId}", alertId);
            return StatusCode(500, new { error = "Internal server error escalating alert" });
        }
    }

    /// <summary>
    /// Get alert statistics
    /// </summary>
    /// <param name="since">Optional date filter</param>
    /// <returns>Alert statistics by severity</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<Dictionary<AlertSeverity, int>>> GetAlertStatisticsAsync(
        [FromQuery] DateTime? since = null)
    {
        try
        {
            var statistics = await _alertingService.GetAlertStatisticsAsync(since);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert statistics");
            return StatusCode(500, new { error = "Internal server error retrieving alert statistics" });
        }
    }
} 