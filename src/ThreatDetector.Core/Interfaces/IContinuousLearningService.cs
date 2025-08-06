namespace ThreatDetector.Core.Interfaces;

public interface IContinuousLearningService
{
    Task<bool> AdaptModelAsync(string modelType, object newData, CancellationToken cancellationToken = default);
    
    Task<bool> RetrainModelAsync(string modelType, IEnumerable<object> trainingData, CancellationToken cancellationToken = default);
    
    Task<double> EvaluateModelPerformanceAsync(string modelType, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateModelWeightsAsync(string modelType, Dictionary<string, double> weights, CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, object>> GetModelMetricsAsync(string modelType, CancellationToken cancellationToken = default);
    
    Task<bool> IsModelRetrainingRequiredAsync(string modelType, CancellationToken cancellationToken = default);
    
    Task ScheduleModelUpdateAsync(string modelType, TimeSpan interval, CancellationToken cancellationToken = default);
    
    Task<bool> RollbackModelAsync(string modelType, string version, CancellationToken cancellationToken = default);
    
    Task<List<string>> GetAvailableModelVersionsAsync(string modelType, CancellationToken cancellationToken = default);
} 