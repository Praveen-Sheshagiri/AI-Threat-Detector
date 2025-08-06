using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ThreatDetectionController : ControllerBase
{
    private readonly IThreatDetectionService _threatDetectionService;
    private readonly ILogger<ThreatDetectionController> _logger;

    public ThreatDetectionController(
        IThreatDetectionService threatDetectionService,
        ILogger<ThreatDetectionController> logger)
    {
        _threatDetectionService = threatDetectionService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze data for potential threats
    /// </summary>
    /// <param name="data">Data to analyze for threats</param>
    /// <returns>Threat analysis result</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<ThreatEvent>> AnalyzeAsync([FromBody] object data)
    {
        try
        {
            var result = await _threatDetectionService.AnalyzeAsync(data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing threat data");
            return StatusCode(500, new { error = "Internal server error analyzing threat data" });
        }
    }

    /// <summary>
    /// Get all active threats
    /// </summary>
    /// <returns>List of active threats</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ThreatEvent>>> GetActiveThreatsAsync()
    {
        try
        {
            var threats = await _threatDetectionService.GetActiveThreatsAsync();
            return Ok(threats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active threats");
            return StatusCode(500, new { error = "Internal server error retrieving threats" });
        }
    }

    /// <summary>
    /// Get threats by type
    /// </summary>
    /// <param name="threatType">Type of threat to filter by</param>
    /// <param name="since">Optional date filter</param>
    /// <returns>List of threats matching the criteria</returns>
    [HttpGet("by-type/{threatType}")]
    public async Task<ActionResult<List<ThreatEvent>>> GetThreatsByTypeAsync(
        string threatType, 
        [FromQuery] DateTime? since = null)
    {
        try
        {
            var threats = await _threatDetectionService.GetThreatsByTypeAsync(threatType, since);
            return Ok(threats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving threats by type {ThreatType}", threatType);
            return StatusCode(500, new { error = "Internal server error retrieving threats" });
        }
    }

    /// <summary>
    /// Calculate threat score for given data
    /// </summary>
    /// <param name="data">Data to calculate threat score for</param>
    /// <returns>Threat score between 0 and 1</returns>
    [HttpPost("score")]
    public async Task<ActionResult<double>> GetThreatScoreAsync([FromBody] object data)
    {
        try
        {
            var score = await _threatDetectionService.GetThreatScoreAsync(data);
            return Ok(new { score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating threat score");
            return StatusCode(500, new { error = "Internal server error calculating threat score" });
        }
    }

    /// <summary>
    /// Mitigate a specific threat
    /// </summary>
    /// <param name="threatId">ID of the threat to mitigate</param>
    /// <param name="action">Mitigation action to take</param>
    /// <returns>Success status</returns>
    [HttpPost("{threatId}/mitigate")]
    public async Task<ActionResult<bool>> MitigateThreateAsync(
        Guid threatId, 
        [FromBody] string action)
    {
        try
        {
            var result = await _threatDetectionService.MitigateThreatAsync(threatId, action);
            return Ok(new { success = result, message = result ? "Threat mitigated successfully" : "Failed to mitigate threat" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mitigating threat {ThreatId}", threatId);
            return StatusCode(500, new { error = "Internal server error mitigating threat" });
        }
    }

    /// <summary>
    /// Update threat status
    /// </summary>
    /// <param name="threatId">ID of the threat to update</param>
    /// <param name="status">New threat status</param>
    /// <returns>Updated threat event</returns>
    [HttpPut("{threatId}/status")]
    public async Task<ActionResult<ThreatEvent>> UpdateThreatStatusAsync(
        Guid threatId, 
        [FromBody] ThreatStatus status)
    {
        try
        {
            var updatedThreat = await _threatDetectionService.UpdateThreatStatusAsync(threatId, status);
            return Ok(updatedThreat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating threat status for {ThreatId}", threatId);
            return StatusCode(500, new { error = "Internal server error updating threat status" });
        }
    }

    /// <summary>
    /// Train the threat detection model with new data
    /// </summary>
    /// <param name="trainingData">Training data for the model</param>
    /// <returns>Success status</returns>
    [HttpPost("train")]
    public async Task<ActionResult> TrainModelAsync([FromBody] List<object> trainingData)
    {
        try
        {
            await _threatDetectionService.TrainModelAsync(trainingData);
            return Ok(new { message = "Model training completed successfully", samplesProcessed = trainingData.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training threat detection model");
            return StatusCode(500, new { error = "Internal server error training model" });
        }
    }

    /// <summary>
    /// Check if a threat is a zero-day vulnerability
    /// </summary>
    /// <param name="threat">Threat to analyze</param>
    /// <returns>Boolean indicating if it's a zero-day threat</returns>
    [HttpPost("check-zero-day")]
    public async Task<ActionResult<bool>> IsZeroDayThreatAsync([FromBody] ThreatEvent threat)
    {
        try
        {
            var isZeroDay = await _threatDetectionService.IsZeroDayThreatAsync(threat);
            return Ok(new { isZeroDay, confidence = threat.ConfidenceScore });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking zero-day threat");
            return StatusCode(500, new { error = "Internal server error checking zero-day threat" });
        }
    }
} 