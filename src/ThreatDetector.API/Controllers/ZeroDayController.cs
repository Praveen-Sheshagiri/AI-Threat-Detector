using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;

namespace ThreatDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ZeroDayController : ControllerBase
{
    private readonly IZeroDayDetectionService _zeroDayService;
    private readonly ILogger<ZeroDayController> _logger;

    public ZeroDayController(
        IZeroDayDetectionService zeroDayService,
        ILogger<ZeroDayController> logger)
    {
        _zeroDayService = zeroDayService;
        _logger = logger;
    }

    /// <summary>
    /// Detect zero-day vulnerabilities in payload
    /// </summary>
    /// <param name="payload">Payload data to analyze</param>
    /// <returns>Zero-day vulnerability if detected</returns>
    [HttpPost("detect")]
    public async Task<ActionResult<ZeroDayVulnerability?>> DetectZeroDayAsync([FromBody] object payload)
    {
        try
        {
            var vulnerability = await _zeroDayService.DetectZeroDayAsync(payload);
            if (vulnerability != null)
            {
                return Ok(vulnerability);
            }
            return Ok(new { message = "No zero-day vulnerability detected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting zero-day vulnerability");
            return StatusCode(500, new { error = "Internal server error detecting zero-day vulnerability" });
        }
    }

    /// <summary>
    /// Get all active zero-day vulnerabilities
    /// </summary>
    /// <returns>List of active vulnerabilities</returns>
    [HttpGet("active")]
    public async Task<ActionResult<List<ZeroDayVulnerability>>> GetActiveVulnerabilitiesAsync()
    {
        try
        {
            var vulnerabilities = await _zeroDayService.GetActiveVulnerabilitiesAsync();
            return Ok(vulnerabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active vulnerabilities");
            return StatusCode(500, new { error = "Internal server error retrieving vulnerabilities" });
        }
    }

    /// <summary>
    /// Update vulnerability status
    /// </summary>
    /// <param name="vulnerabilityId">Vulnerability ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated vulnerability</returns>
    [HttpPut("{vulnerabilityId}/status")]
    public async Task<ActionResult<ZeroDayVulnerability>> UpdateVulnerabilityStatusAsync(
        Guid vulnerabilityId,
        [FromBody] VulnerabilityStatus status)
    {
        try
        {
            var updatedVulnerability = await _zeroDayService.UpdateVulnerabilityStatusAsync(vulnerabilityId, status);
            return Ok(updatedVulnerability);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vulnerability status for {VulnerabilityId}", vulnerabilityId);
            return StatusCode(500, new { error = "Internal server error updating vulnerability status" });
        }
    }

    /// <summary>
    /// Validate a vulnerability
    /// </summary>
    /// <param name="vulnerabilityId">Vulnerability ID to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("{vulnerabilityId}/validate")]
    public async Task<ActionResult<bool>> ValidateVulnerabilityAsync(Guid vulnerabilityId)
    {
        try
        {
            var isValid = await _zeroDayService.ValidateVulnerabilityAsync(vulnerabilityId);
            return Ok(new { vulnerabilityId, isValid, message = isValid ? "Vulnerability validated" : "Validation failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating vulnerability {VulnerabilityId}", vulnerabilityId);
            return StatusCode(500, new { error = "Internal server error validating vulnerability" });
        }
    }

    /// <summary>
    /// Search vulnerabilities by signature
    /// </summary>
    /// <param name="signature">Signature to search for</param>
    /// <returns>List of matching vulnerabilities</returns>
    [HttpGet("search")]
    public async Task<ActionResult<List<ZeroDayVulnerability>>> SearchVulnerabilitiesBySignatureAsync(
        [FromQuery] string signature)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signature))
            {
                return BadRequest(new { error = "Signature parameter is required" });
            }

            var vulnerabilities = await _zeroDayService.SearchVulnerabilitiesBySignatureAsync(signature);
            return Ok(vulnerabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vulnerabilities by signature {Signature}", signature);
            return StatusCode(500, new { error = "Internal server error searching vulnerabilities" });
        }
    }

    /// <summary>
    /// Calculate vulnerability score for exploit data
    /// </summary>
    /// <param name="exploitData">Exploit data to analyze</param>
    /// <returns>Vulnerability score</returns>
    [HttpPost("calculate-score")]
    public async Task<ActionResult<double>> CalculateVulnerabilityScoreAsync([FromBody] object exploitData)
    {
        try
        {
            var score = await _zeroDayService.CalculateVulnerabilityScoreAsync(exploitData);
            return Ok(new { vulnerabilityScore = score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating vulnerability score");
            return StatusCode(500, new { error = "Internal server error calculating vulnerability score" });
        }
    }

    /// <summary>
    /// Train zero-day detection model
    /// </summary>
    /// <param name="knownExploits">Known exploit data for training</param>
    /// <returns>Training result</returns>
    [HttpPost("train")]
    public async Task<ActionResult> TrainDetectionModelAsync([FromBody] List<object> knownExploits)
    {
        try
        {
            await _zeroDayService.TrainDetectionModelAsync(knownExploits);
            return Ok(new { 
                message = "Zero-day detection model trained successfully", 
                samplesProcessed = knownExploits.Count 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training zero-day detection model");
            return StatusCode(500, new { error = "Internal server error training detection model" });
        }
    }

    /// <summary>
    /// Correlate vulnerability with known vulnerabilities
    /// </summary>
    /// <param name="vulnerabilityId">Vulnerability ID to correlate</param>
    /// <returns>Correlation result</returns>
    [HttpPost("{vulnerabilityId}/correlate")]
    public async Task<ActionResult<bool>> CorrelateWithKnownVulnerabilitiesAsync(Guid vulnerabilityId)
    {
        try
        {
            // First get the vulnerability
            var vulnerabilities = await _zeroDayService.GetActiveVulnerabilitiesAsync();
            var vulnerability = vulnerabilities.FirstOrDefault(v => v.Id == vulnerabilityId);
            
            if (vulnerability == null)
            {
                return NotFound(new { error = "Vulnerability not found" });
            }

            var hasCorrelation = await _zeroDayService.CorrelateWithKnownVulnerabilitiesAsync(vulnerability);
            return Ok(new { 
                vulnerabilityId, 
                hasCorrelation, 
                message = hasCorrelation ? "Correlation found with known vulnerabilities" : "No correlation found" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error correlating vulnerability {VulnerabilityId}", vulnerabilityId);
            return StatusCode(500, new { error = "Internal server error correlating vulnerability" });
        }
    }
} 