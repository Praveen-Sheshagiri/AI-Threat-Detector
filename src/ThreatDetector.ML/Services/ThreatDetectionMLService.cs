using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Logging;
using ThreatDetector.Core.Interfaces;
using ThreatDetector.Core.Models;
using ThreatDetector.ML.Models;
using System.Text.Json;

namespace ThreatDetector.ML.Services;

public class ThreatDetectionMLService : IThreatDetectionService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<ThreatDetectionMLService> _logger;
    private ITransformer? _model;
    private PredictionEngine<ThreatPredictionInput, ThreatPredictionOutput>? _predictionEngine;
    private readonly string _modelPath = "Models/threat-detection-model.zip";

    public ThreatDetectionMLService(ILogger<ThreatDetectionMLService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        LoadOrCreateModel();
    }

    public async Task<ThreatEvent> AnalyzeAsync(object data, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = ConvertToMLInput(data);
            var prediction = _predictionEngine?.Predict(input);

            if (prediction == null)
            {
                throw new InvalidOperationException("Prediction engine not initialized");
            }

            var threatEvent = new ThreatEvent
            {
                ThreatType = DetermineThreatType(input, prediction),
                Severity = DetermineSeverity(prediction.Probability),
                Source = input.SourceIp ?? "Unknown",
                Target = input.DestinationIp ?? "Unknown",
                Description = GenerateDescription(input, prediction),
                ConfidenceScore = prediction.Probability,
                IsZeroDay = await IsZeroDayThreatAsync(new ThreatEvent(), cancellationToken),
                Metadata = ExtractMetadata(input, prediction)
            };

            _logger.LogInformation("Threat analysis completed. Threat detected: {IsThreat}, Confidence: {Confidence}",
                prediction.IsThreat, prediction.Probability);

            return threatEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing threat data");
            throw;
        }
    }

    public async Task<List<ThreatEvent>> GetActiveThreatsAsync(CancellationToken cancellationToken = default)
    {
        // This would typically query a database
        // For now, return empty list as this is a demo
        return new List<ThreatEvent>();
    }

    public async Task<bool> MitigateThreatAsync(Guid threatId, string action, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mitigating threat {ThreatId} with action: {Action}", threatId, action);
        
        // Implement mitigation logic
        await Task.Delay(100, cancellationToken); // Simulate processing
        return true;
    }

    public async Task<ThreatEvent> UpdateThreatStatusAsync(Guid threatId, ThreatStatus status, CancellationToken cancellationToken = default)
    {
        // This would typically update the database
        await Task.Delay(50, cancellationToken);
        
        return new ThreatEvent
        {
            Id = threatId,
            Status = status
        };
    }

    public async Task<List<ThreatEvent>> GetThreatsByTypeAsync(string threatType, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        // This would typically query a database
        await Task.Delay(50, cancellationToken);
        return new List<ThreatEvent>();
    }

    public async Task<double> GetThreatScoreAsync(object data, CancellationToken cancellationToken = default)
    {
        var input = ConvertToMLInput(data);
        var prediction = _predictionEngine?.Predict(input);
        
        await Task.Delay(10, cancellationToken);
        return prediction?.Probability ?? 0.0;
    }

    public async Task TrainModelAsync(IEnumerable<object> trainingData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting model training with {Count} samples", trainingData.Count());
        
        var mlData = trainingData.Select(ConvertToMLInput).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(mlData);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("PayloadDataFeatures", nameof(ThreatPredictionInput.PayloadData))
            .Append(_mlContext.Transforms.Text.FeaturizeText("SourceIpFeatures", nameof(ThreatPredictionInput.SourceIp)))
            .Append(_mlContext.Transforms.Concatenate("Features", 
                "PayloadDataFeatures", "SourceIpFeatures",
                nameof(ThreatPredictionInput.PayloadSize),
                nameof(ThreatPredictionInput.PacketCount),
                nameof(ThreatPredictionInput.BytesPerSecond),
                nameof(ThreatPredictionInput.Port),
                nameof(ThreatPredictionInput.RequestFrequency),
                nameof(ThreatPredictionInput.ResponseTime),
                nameof(ThreatPredictionInput.SessionDuration),
                nameof(ThreatPredictionInput.TimeOfDay),
                nameof(ThreatPredictionInput.EntropyScore),
                nameof(ThreatPredictionInput.AnomalyScore)))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<ThreatPredictionInput, ThreatPredictionOutput>(_model);

        // Save the model
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        
        _logger.LogInformation("Model training completed and saved");
        await Task.CompletedTask;
    }

    public async Task<bool> IsZeroDayThreatAsync(ThreatEvent threat, CancellationToken cancellationToken = default)
    {
        // Simple heuristic - in a real implementation, this would be more sophisticated
        await Task.Delay(50, cancellationToken);
        return threat.ConfidenceScore > 0.8 && string.IsNullOrEmpty(threat.ThreatType);
    }

    private void LoadOrCreateModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<ThreatPredictionInput, ThreatPredictionOutput>(_model);
                _logger.LogInformation("Loaded existing threat detection model");
            }
            else
            {
                _logger.LogWarning("No existing model found. Please train the model first.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading threat detection model");
        }
    }

    private ThreatPredictionInput ConvertToMLInput(object data)
    {
        var json = JsonSerializer.Serialize(data);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();

        return new ThreatPredictionInput
        {
            PayloadData = dict.GetValueOrDefault("payloadData")?.ToString(),
            SourceIp = dict.GetValueOrDefault("sourceIp")?.ToString(),
            DestinationIp = dict.GetValueOrDefault("destinationIp")?.ToString(),
            PayloadSize = Convert.ToSingle(dict.GetValueOrDefault("payloadSize", 0)),
            PacketCount = Convert.ToSingle(dict.GetValueOrDefault("packetCount", 0)),
            BytesPerSecond = Convert.ToSingle(dict.GetValueOrDefault("bytesPerSecond", 0)),
            Protocol = dict.GetValueOrDefault("protocol")?.ToString(),
            Port = Convert.ToSingle(dict.GetValueOrDefault("port", 0)),
            UserAgent = dict.GetValueOrDefault("userAgent")?.ToString(),
            RequestFrequency = Convert.ToSingle(dict.GetValueOrDefault("requestFrequency", 0)),
            RequestMethod = dict.GetValueOrDefault("requestMethod")?.ToString(),
            ResponseTime = Convert.ToSingle(dict.GetValueOrDefault("responseTime", 0)),
            HttpStatusCode = dict.GetValueOrDefault("httpStatusCode")?.ToString(),
            SessionDuration = Convert.ToSingle(dict.GetValueOrDefault("sessionDuration", 0)),
            GeoLocation = dict.GetValueOrDefault("geoLocation")?.ToString(),
            TimeOfDay = Convert.ToSingle(dict.GetValueOrDefault("timeOfDay", 0)),
            DeviceFingerprint = dict.GetValueOrDefault("deviceFingerprint")?.ToString(),
            EntropyScore = Convert.ToSingle(dict.GetValueOrDefault("entropyScore", 0)),
            AnomalyScore = Convert.ToSingle(dict.GetValueOrDefault("anomalyScore", 0))
        };
    }

    private string DetermineThreatType(ThreatPredictionInput input, ThreatPredictionOutput prediction)
    {
        if (input.EntropyScore > 0.8) return "Malware";
        if (input.RequestFrequency > 100) return "DDoS";
        if (input.AnomalyScore > 0.7) return "Anomalous Behavior";
        if (prediction.Probability > 0.9) return "High Confidence Threat";
        return "Suspicious Activity";
    }

    private ThreatSeverity DetermineSeverity(float probability)
    {
        return probability switch
        {
            >= 0.9f => ThreatSeverity.Critical,
            >= 0.7f => ThreatSeverity.High,
            >= 0.5f => ThreatSeverity.Medium,
            _ => ThreatSeverity.Low
        };
    }

    private string GenerateDescription(ThreatPredictionInput input, ThreatPredictionOutput prediction)
    {
        return $"Threat detected with {prediction.Probability:P2} confidence. Source: {input.SourceIp}, Target: {input.DestinationIp}";
    }

    private Dictionary<string, object> ExtractMetadata(ThreatPredictionInput input, ThreatPredictionOutput prediction)
    {
        return new Dictionary<string, object>
        {
            ["protocol"] = input.Protocol ?? "Unknown",
            ["port"] = input.Port,
            ["payloadSize"] = input.PayloadSize,
            ["entropyScore"] = input.EntropyScore,
            ["modelScore"] = prediction.Score,
            ["modelProbability"] = prediction.Probability
        };
    }
} 