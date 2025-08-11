using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Security.Claims;

namespace ThreatDetector.SDK;

/// <summary>
/// Interface for Threat Detector middleware
/// </summary>
public interface IThreatDetectorMiddleware
{
    Task InvokeAsync(HttpContext context, RequestDelegate next);
}

/// <summary>
/// Middleware for automatic threat detection on HTTP requests
/// </summary>
public class ThreatDetectorMiddleware : IThreatDetectorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IThreatDetectorClient _threatDetectorClient;
    private readonly ThreatDetectorOptions _options;
    private readonly ILogger<ThreatDetectorMiddleware> _logger;

    public ThreatDetectorMiddleware(
        RequestDelegate next,
        IThreatDetectorClient threatDetectorClient,
        IOptions<ThreatDetectorOptions> options,
        ILogger<ThreatDetectorMiddleware> logger)
    {
        _next = next;
        _threatDetectorClient = threatDetectorClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Capture request data for analysis
            var requestData = await CaptureRequestDataAsync(context);
            
            // Analyze request for threats
            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(requestData, "HTTP_Request");
            
            // Log threat detection result
            if (threatResult.IsThreat && threatResult.ThreatScore >= _options.ThreatThreshold)
            {
                _logger.LogWarning("Potential threat detected in HTTP request. Score: {ThreatScore}, Type: {ThreatType}", 
                    threatResult.ThreatScore, threatResult.ThreatType);
                
                // Add threat information to response headers (for debugging)
                context.Response.Headers.Add("X-Threat-Score", threatResult.ThreatScore.ToString("F2"));
                context.Response.Headers.Add("X-Threat-Type", threatResult.ThreatType);
                
                // Handle high-severity threats
                if (threatResult.Severity == "Critical" || threatResult.Severity == "High")
                {
                    await HandleHighSeverityThreatAsync(context, threatResult);
                    return;
                }
            }

            // Analyze user behavior if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = GetUserId(context);
                if (!string.IsNullOrEmpty(userId))
                {
                    var behaviorData = CreateUserBehaviorData(context, requestData);
                    var behaviorResult = await _threatDetectorClient.AnalyzeUserBehaviorAsync(userId, behaviorData);
                    
                    if (behaviorResult.IsAnomalous)
                    {
                        _logger.LogWarning("Anomalous user behavior detected for user {UserId}. Score: {AnomalyScore}", 
                            userId, behaviorResult.AnomalyScore);
                        
                        context.Response.Headers.Add("X-Behavior-Score", behaviorResult.AnomalyScore.ToString("F2"));
                    }
                }
            }

            // Proceed with the request
            await _next(context);
            
            // Analyze response for zero-day vulnerabilities if enabled
            if (_options.EnableZeroDayDetection && context.Response.StatusCode >= 400)
            {
                var responseData = CreateResponseData(context, startTime);
                var zeroDayResult = await _threatDetectorClient.DetectZeroDayAsync(responseData);
                
                if (zeroDayResult.IsZeroDay)
                {
                    _logger.LogError("Potential zero-day vulnerability detected in response. Confidence: {ConfidenceScore}", 
                        zeroDayResult.ConfidenceScore);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ThreatDetectorMiddleware");
            // Continue with request even if threat detection fails
            await _next(context);
        }
    }

    private async Task<Dictionary<string, object>> CaptureRequestDataAsync(HttpContext context)
    {
        var request = context.Request;
        var data = new Dictionary<string, object>
        {
            ["method"] = request.Method,
            ["path"] = request.Path.Value ?? "",
            ["queryString"] = request.QueryString.Value ?? "",
            ["userAgent"] = request.Headers.UserAgent.ToString(),
            ["remoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "",
            ["timestamp"] = DateTime.UtcNow,
            ["contentLength"] = request.ContentLength ?? 0,
            ["contentType"] = request.ContentType ?? "",
            ["headers"] = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            ["protocol"] = request.Protocol,
            ["scheme"] = request.Scheme,
            ["host"] = request.Host.Value,
            ["isHttps"] = request.IsHttps
        };

        // Capture request body for POST/PUT requests (be careful with large payloads)
        if (request.Method == "POST" || request.Method == "PUT")
        {
            if (request.ContentLength.HasValue && request.ContentLength.Value < 1024 * 1024) // 1MB limit
            {
                request.EnableBuffering();
                var bodyStream = new MemoryStream();
                await request.Body.CopyToAsync(bodyStream);
                request.Body.Position = 0;
                
                data["bodySize"] = bodyStream.Length;
                data["hasBody"] = bodyStream.Length > 0;
            }
        }

        return data;
    }

    private static Dictionary<string, object> CreateUserBehaviorData(HttpContext context, Dictionary<string, object> requestData)
    {
        return new Dictionary<string, object>
        {
            ["eventType"] = "HttpRequest",
            ["ipAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "",
            ["userAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["timestamp"] = DateTime.UtcNow,
            ["requestMethod"] = context.Request.Method,
            ["requestPath"] = context.Request.Path.Value ?? "",
            ["requestSize"] = context.Request.ContentLength ?? 0,
            ["sessionId"] = context.Session?.Id ?? "",
            ["deviceFingerprint"] = GenerateDeviceFingerprint(context),
            ["requestData"] = requestData
        };
    }

    private static Dictionary<string, object> CreateResponseData(HttpContext context, DateTime startTime)
    {
        return new Dictionary<string, object>
        {
            ["statusCode"] = context.Response.StatusCode,
            ["responseTime"] = (DateTime.UtcNow - startTime).TotalMilliseconds,
            ["contentLength"] = context.Response.ContentLength ?? 0,
            ["contentType"] = context.Response.ContentType ?? "",
            ["headers"] = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            ["timestamp"] = DateTime.UtcNow
        };
    }

    private static string GetUserId(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               context.User.FindFirst("sub")?.Value ??
               context.User.FindFirst("id")?.Value ??
               "";
    }

    private static string GenerateDeviceFingerprint(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        var acceptEncoding = context.Request.Headers.AcceptEncoding.ToString();
        
        var combined = $"{userAgent}|{acceptLanguage}|{acceptEncoding}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(combined));
    }

    private async Task HandleHighSeverityThreatAsync(HttpContext context, ThreatDetectionResult threatResult)
    {
        _logger.LogError("High severity threat blocked: {ThreatType} with score {ThreatScore}", 
            threatResult.ThreatType, threatResult.ThreatScore);

        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Request blocked due to security threat",
            threatId = threatResult.ThreatId,
            threatType = threatResult.ThreatType,
            message = "Your request has been identified as a potential security threat and has been blocked.",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// Extension methods for configuring the Threat Detector middleware
/// </summary>
public static class ThreatDetectorMiddlewareExtensions
{
    /// <summary>
    /// Adds the Threat Detector middleware to the pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseThreatDetector(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ThreatDetectorMiddleware>();
    }
}
