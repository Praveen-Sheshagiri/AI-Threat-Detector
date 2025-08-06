using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserBehaviorController : ControllerBase
{
    private readonly IUserBehaviorAnalysisService _userBehaviorService;
    private readonly ILogger<UserBehaviorController> _logger;

    public UserBehaviorController(
        IUserBehaviorAnalysisService userBehaviorService,
        ILogger<UserBehaviorController> logger)
    {
        _userBehaviorService = userBehaviorService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze user behavior for anomalies
    /// </summary>
    /// <param name="userId">User ID to analyze</param>
    /// <param name="eventData">User event data</param>
    /// <returns>User behavior analysis result</returns>
    [HttpPost("analyze/{userId}")]
    public async Task<ActionResult<UserBehaviorEvent>> AnalyzeBehaviorAsync(
        string userId, 
        [FromBody] object eventData)
    {
        try
        {
            var result = await _userBehaviorService.AnalyzeBehaviorAsync(userId, eventData);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user behavior for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error analyzing user behavior" });
        }
    }

    /// <summary>
    /// Get anomalous behavior events
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="since">Optional date filter</param>
    /// <returns>List of anomalous behavior events</returns>
    [HttpGet("anomalies")]
    public async Task<ActionResult<List<UserBehaviorEvent>>> GetAnomalousBehaviorAsync(
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? since = null)
    {
        try
        {
            var anomalies = await _userBehaviorService.GetAnomalousBehaviorAsync(userId, since);
            return Ok(anomalies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving anomalous behavior");
            return StatusCode(500, new { error = "Internal server error retrieving anomalies" });
        }
    }

    /// <summary>
    /// Calculate anomaly score for user behavior
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="eventData">Event data to analyze</param>
    /// <returns>Anomaly score between 0 and 1</returns>
    [HttpPost("anomaly-score/{userId}")]
    public async Task<ActionResult<double>> CalculateAnomalyScoreAsync(
        string userId, 
        [FromBody] object eventData)
    {
        try
        {
            var score = await _userBehaviorService.CalculateAnomalyScoreAsync(userId, eventData);
            return Ok(new { userId, anomalyScore = score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating anomaly score for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error calculating anomaly score" });
        }
    }

    /// <summary>
    /// Update user baseline behavior
    /// </summary>
    /// <param name="userId">User ID to update baseline for</param>
    /// <returns>Success status</returns>
    [HttpPost("update-baseline/{userId}")]
    public async Task<ActionResult> UpdateUserBaselineAsync(string userId)
    {
        try
        {
            await _userBehaviorService.UpdateUserBaselineAsync(userId);
            return Ok(new { message = $"Baseline updated successfully for user {userId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating baseline for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error updating user baseline" });
        }
    }

    /// <summary>
    /// Get user behavior history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="since">Optional date filter</param>
    /// <param name="limit">Optional result limit</param>
    /// <returns>User behavior history</returns>
    [HttpGet("history/{userId}")]
    public async Task<ActionResult<List<UserBehaviorEvent>>> GetUserBehaviorHistoryAsync(
        string userId,
        [FromQuery] DateTime? since = null,
        [FromQuery] int? limit = null)
    {
        try
        {
            var history = await _userBehaviorService.GetUserBehaviorHistoryAsync(userId, since, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving behavior history for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error retrieving behavior history" });
        }
    }

    /// <summary>
    /// Assess risk level for a user
    /// </summary>
    /// <param name="userId">User ID to assess</param>
    /// <returns>User risk level</returns>
    [HttpGet("risk-level/{userId}")]
    public async Task<ActionResult<BehaviorRiskLevel>> AssessRiskLevelAsync(string userId)
    {
        try
        {
            var riskLevel = await _userBehaviorService.AssessRiskLevelAsync(userId);
            return Ok(new { userId, riskLevel });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing risk level for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error assessing risk level" });
        }
    }

    /// <summary>
    /// Train behavior model for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="historicalData">Historical behavior data for training</param>
    /// <returns>Success status</returns>
    [HttpPost("train/{userId}")]
    public async Task<ActionResult> TrainBehaviorModelAsync(
        string userId, 
        [FromBody] List<UserBehaviorEvent> historicalData)
    {
        try
        {
            await _userBehaviorService.TrainBehaviorModelAsync(userId, historicalData);
            return Ok(new { 
                message = $"Behavior model trained successfully for user {userId}", 
                samplesProcessed = historicalData.Count 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training behavior model for {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error training behavior model" });
        }
    }

    /// <summary>
    /// Check if a session is high risk
    /// </summary>
    /// <param name="sessionId">Session ID to check</param>
    /// <returns>Boolean indicating if session is high risk</returns>
    [HttpGet("high-risk-session/{sessionId}")]
    public async Task<ActionResult<bool>> IsHighRiskSessionAsync(string sessionId)
    {
        try
        {
            var isHighRisk = await _userBehaviorService.IsHighRiskSessionAsync(sessionId);
            return Ok(new { sessionId, isHighRisk });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking high risk session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Internal server error checking session risk" });
        }
    }
} 