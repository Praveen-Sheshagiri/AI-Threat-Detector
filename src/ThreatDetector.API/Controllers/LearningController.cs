using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Core.Interfaces;

namespace ThreatDetector.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LearningController : ControllerBase
{
    private readonly IContinuousLearningService _learningService;
    private readonly ILogger<LearningController> _logger;

    public LearningController(
        IContinuousLearningService learningService,
        ILogger<LearningController> logger)
    {
        _learningService = learningService;
        _logger = logger;
    }

    /// <summary>
    /// Adapt model with new data point
    /// </summary>
    /// <param name="modelType">Type of model to adapt</param>
    /// <param name="newData">New data for adaptation</param>
    /// <returns>Success status</returns>
    [HttpPost("adapt/{modelType}")]
    public async Task<ActionResult> AdaptModelAsync(string modelType, [FromBody] object newData)
    {
        try
        {
            var success = await _learningService.AdaptModelAsync(modelType, newData);
            if (success)
            {
                return Ok(new { message = $"Model {modelType} adapted successfully" });
            }
            return BadRequest(new { error = $"Model {modelType} not found or adaptation failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adapting model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error adapting model" });
        }
    }

    /// <summary>
    /// Retrain model with training data
    /// </summary>
    /// <param name="modelType">Type of model to retrain</param>
    /// <param name="trainingData">Training data</param>
    /// <returns>Success status</returns>
    [HttpPost("retrain/{modelType}")]
    public async Task<ActionResult> RetrainModelAsync(string modelType, [FromBody] List<object> trainingData)
    {
        try
        {
            var success = await _learningService.RetrainModelAsync(modelType, trainingData);
            if (success)
            {
                return Ok(new { 
                    message = $"Model {modelType} retrained successfully", 
                    samplesProcessed = trainingData.Count 
                });
            }
            return BadRequest(new { error = $"Model {modelType} not found or retraining failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retraining model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error retraining model" });
        }
    }

    /// <summary>
    /// Evaluate model performance
    /// </summary>
    /// <param name="modelType">Type of model to evaluate</param>
    /// <returns>Performance score</returns>
    [HttpGet("performance/{modelType}")]
    public async Task<ActionResult<double>> EvaluateModelPerformanceAsync(string modelType)
    {
        try
        {
            var performance = await _learningService.EvaluateModelPerformanceAsync(modelType);
            return Ok(new { modelType, performance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating model performance for {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error evaluating model performance" });
        }
    }

    /// <summary>
    /// Update model weights
    /// </summary>
    /// <param name="modelType">Type of model to update</param>
    /// <param name="weights">New weights</param>
    /// <returns>Success status</returns>
    [HttpPut("weights/{modelType}")]
    public async Task<ActionResult> UpdateModelWeightsAsync(
        string modelType, 
        [FromBody] Dictionary<string, double> weights)
    {
        try
        {
            var success = await _learningService.UpdateModelWeightsAsync(modelType, weights);
            if (success)
            {
                return Ok(new { 
                    message = $"Weights updated successfully for model {modelType}", 
                    weightsUpdated = weights.Count 
                });
            }
            return BadRequest(new { error = $"Model {modelType} not found or weight update failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weights for model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error updating model weights" });
        }
    }

    /// <summary>
    /// Get model metrics
    /// </summary>
    /// <param name="modelType">Type of model to get metrics for</param>
    /// <returns>Model metrics</returns>
    [HttpGet("metrics/{modelType}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetModelMetricsAsync(string modelType)
    {
        try
        {
            var metrics = await _learningService.GetModelMetricsAsync(modelType);
            if (metrics.Any())
            {
                return Ok(metrics);
            }
            return NotFound(new { error = $"Model {modelType} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics for model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error getting model metrics" });
        }
    }

    /// <summary>
    /// Check if model requires retraining
    /// </summary>
    /// <param name="modelType">Type of model to check</param>
    /// <returns>Retraining requirement status</returns>
    [HttpGet("retraining-required/{modelType}")]
    public async Task<ActionResult<bool>> IsModelRetrainingRequiredAsync(string modelType)
    {
        try
        {
            var retrainingRequired = await _learningService.IsModelRetrainingRequiredAsync(modelType);
            return Ok(new { modelType, retrainingRequired });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking retraining requirement for model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error checking retraining requirement" });
        }
    }

    /// <summary>
    /// Schedule model updates
    /// </summary>
    /// <param name="modelType">Type of model to schedule updates for</param>
    /// <param name="intervalHours">Update interval in hours</param>
    /// <returns>Success status</returns>
    [HttpPost("schedule/{modelType}")]
    public async Task<ActionResult> ScheduleModelUpdateAsync(
        string modelType, 
        [FromBody] int intervalHours)
    {
        try
        {
            var interval = TimeSpan.FromHours(intervalHours);
            await _learningService.ScheduleModelUpdateAsync(modelType, interval);
            return Ok(new { 
                message = $"Model {modelType} scheduled for updates every {intervalHours} hours" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling updates for model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error scheduling model updates" });
        }
    }

    /// <summary>
    /// Rollback model to previous version
    /// </summary>
    /// <param name="modelType">Type of model to rollback</param>
    /// <param name="version">Version to rollback to</param>
    /// <returns>Success status</returns>
    [HttpPost("rollback/{modelType}")]
    public async Task<ActionResult> RollbackModelAsync(
        string modelType, 
        [FromBody] string version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return BadRequest(new { error = "Version parameter is required" });
            }

            var success = await _learningService.RollbackModelAsync(modelType, version);
            if (success)
            {
                return Ok(new { message = $"Model {modelType} rolled back to version {version}" });
            }
            return BadRequest(new { error = $"Model {modelType} rollback failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back model {ModelType} to version {Version}", modelType, version);
            return StatusCode(500, new { error = "Internal server error rolling back model" });
        }
    }

    /// <summary>
    /// Get available model versions
    /// </summary>
    /// <param name="modelType">Type of model to get versions for</param>
    /// <returns>List of available versions</returns>
    [HttpGet("versions/{modelType}")]
    public async Task<ActionResult<List<string>>> GetAvailableModelVersionsAsync(string modelType)
    {
        try
        {
            var versions = await _learningService.GetAvailableModelVersionsAsync(modelType);
            return Ok(new { modelType, versions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available versions for model {ModelType}", modelType);
            return StatusCode(500, new { error = "Internal server error getting model versions" });
        }
    }

    /// <summary>
    /// Get all model types and their status
    /// </summary>
    /// <returns>Model status overview</returns>
    [HttpGet("overview")]
    public async Task<ActionResult> GetModelsOverviewAsync()
    {
        try
        {
            var modelTypes = new[] { "ThreatDetection", "UserBehaviorAnalysis", "ZeroDayDetection", "AnomalyDetection" };
            var overview = new List<object>();

            foreach (var modelType in modelTypes)
            {
                var metrics = await _learningService.GetModelMetricsAsync(modelType);
                var retrainingRequired = await _learningService.IsModelRetrainingRequiredAsync(modelType);

                overview.Add(new
                {
                    modelType,
                    performance = metrics.GetValueOrDefault("performance", 0.0),
                    version = metrics.GetValueOrDefault("version", "Unknown"),
                    lastUpdated = metrics.GetValueOrDefault("lastUpdated"),
                    retrainingRequired,
                    queueSize = metrics.GetValueOrDefault("queueSize", 0)
                });
            }

            return Ok(new { models = overview, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting models overview");
            return StatusCode(500, new { error = "Internal server error getting models overview" });
        }
    }
} 