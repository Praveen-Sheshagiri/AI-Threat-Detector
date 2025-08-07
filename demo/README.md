# AI Threat Detector - Demo Application & Integration Guide

This demo application showcases how to integrate the AI Threat Detector SDK into any application for comprehensive security monitoring and threat detection.

## 🎯 Overview

The AI Threat Detector system provides:

- **Real-time Threat Detection**: ML-powered analysis of all application activities
- **User Behavior Monitoring**: Anomaly detection for unusual user patterns  
- **Zero-Day Vulnerability Detection**: Advanced algorithms to identify unknown threats
- **Automatic Integration**: Simple SDK integration with minimal code changes
- **Live Monitoring**: Real-time notifications and threat alerts
- **Comprehensive Logging**: Detailed security event tracking

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Your App     │    │  Threat Detector │    │  AI Detection   │
│                │    │       SDK        │    │     Engine      │
│  ┌──────────┐  │    │                  │    │                 │
│  │Middleware│──┼────┼──HTTP Analyzer───┼────┼─ ML Models ────│
│  └──────────┘  │    │                  │    │                 │
│  ┌──────────┐  │    │                  │    │ ┌─────────────┐ │
│  │Controllers──┼────┼──API Client──────┼────┼─│ Threat DB   │ │
│  └──────────┘  │    │                  │    │ └─────────────┘ │
│  ┌──────────┐  │    │                  │    │                 │
│  │Services  │──┼────┼──Event Handlers──┼────┼─ Real-time Hub │
│  └──────────┘  │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## 🚀 Quick Start

### 1. Install the SDK

```bash
dotnet add package ThreatDetector.SDK
```

### 2. Basic Integration

Add to your `Program.cs` or `Startup.cs`:

```csharp
using ThreatDetector.SDK.Extensions;

// Add services
builder.Services.AddThreatDetector(builder.Configuration);

// Add middleware
app.UseThreatDetector();
```

### 3. Configuration

Add to your `appsettings.json`:

```json
{
  "ThreatDetector": {
    "ApiBaseUrl": "https://localhost:7001",
    "ApplicationId": "YourApp",
    "EnableRealTimeNotifications": true,
    "ThreatThreshold": 0.7,
    "EnableUserBehaviorMonitoring": true,
    "EnableZeroDayDetection": true
  }
}
```

## 📋 Demo Application Features

### Threat Detection Examples

The demo application simulates various real-world scenarios:

#### 1. **Web Application Security**
- SQL Injection detection in search queries
- XSS attack prevention in user inputs
- CSRF token validation
- File upload security scanning

#### 2. **User Authentication Security**
- Login anomaly detection
- Brute force attack prevention
- Multi-factor authentication triggers
- Suspicious device detection

#### 3. **API Security**
- Rate limiting and DDoS protection
- Malicious payload detection
- Data exfiltration monitoring
- Privilege escalation attempts

#### 4. **Zero-Day Protection**
- Unknown vulnerability detection
- Behavioral analysis for new threats
- Memory corruption detection
- Advanced persistent threat (APT) identification

## 🛡️ Integration Examples

### Manual Threat Analysis

```csharp
public class ProductController : ControllerBase
{
    private readonly IThreatDetectorClient _threatDetector;

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        // Analyze search query for threats
        var searchData = new
        {
            query = request.Query,
            userAgent = Request.Headers.UserAgent.ToString(),
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _threatDetector.AnalyzeThreatAsync(searchData);
        
        if (result.IsThreat && result.ThreatScore > 0.8)
        {
            return BadRequest(new { 
                error = "Query blocked due to security concerns",
                threatId = result.ThreatId
            });
        }

        // Continue with normal processing
        return Ok(await _productService.SearchAsync(request.Query));
    }
}
```

### User Behavior Monitoring

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Attempt login
    var loginResult = await _userService.LoginAsync(request.Username, request.Password);
    
    if (loginResult.IsSuccess)
    {
        // Analyze login behavior
        var behaviorData = new
        {
            eventType = "Login",
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            userAgent = Request.Headers.UserAgent.ToString(),
            timestamp = DateTime.UtcNow
        };

        var behaviorResult = await _threatDetector.AnalyzeUserBehaviorAsync(
            loginResult.User.Id.ToString(), behaviorData);
        
        if (behaviorResult.IsAnomalous)
        {
            // Trigger additional security measures
            return Ok(new { 
                success = true, 
                warning = "Unusual activity detected. Please verify your identity.",
                requiresMfa = true
            });
        }
    }
    
    return Ok(new { success = loginResult.IsSuccess });
}
```

### Real-Time Event Handling

```csharp
public class SecurityService
{
    private readonly IThreatDetectorClient _threatDetector;
    
    public async Task StartMonitoringAsync()
    {
        // Set up event handlers
        _threatDetector.ThreatDetected += OnThreatDetected;
        _threatDetector.UserBehaviorAnomalyDetected += OnBehaviorAnomaly;
        _threatDetector.ZeroDayDetected += OnZeroDayDetected;
        
        // Start real-time monitoring
        await _threatDetector.StartRealTimeMonitoringAsync();
    }
    
    private async Task OnThreatDetected(ThreatDetectionResult threat)
    {
        // Log threat
        _logger.LogWarning("Threat detected: {ThreatType} with score {ThreatScore}", 
            threat.ThreatType, threat.ThreatScore);
        
        // Send alert to security team
        await _notificationService.SendSecurityAlertAsync(threat);
        
        // Automatic mitigation for critical threats
        if (threat.Severity == "Critical")
        {
            await _securityService.InitiateLockdownAsync(threat.ThreatId);
        }
    }
}
```

## 🔧 Running the Demo

### Prerequisites
- .NET 8.0 SDK
- AI Threat Detector API running on localhost:7001

### Start the Demo

1. **Start the main Threat Detector API**:
   ```bash
   cd src/ThreatDetector.API
   dotnet run
   ```

2. **Run the demo application**:
   ```bash
   cd demo
   dotnet run
   ```

3. **Access the demo**:
   - Swagger UI: `https://localhost:5001/swagger`
   - Demo endpoints: `https://localhost:5001/api/`

### Demo Endpoints

#### Product Management (Web Security Demo)
- `GET /api/products` - Browse products with threat analysis
- `POST /api/products/search` - Search with SQL injection detection
- `POST /api/products` - Create product with content analysis
- `PUT /api/products/bulk` - Bulk operations with abuse detection

#### User Management (Authentication Security Demo)
- `POST /api/users/login` - Login with behavior analysis
- `POST /api/users/register` - Registration with validation
- `GET /api/users/{id}` - Profile access monitoring
- `PUT /api/users/{id}` - Profile update security

#### Threat Simulation (Security Testing Demo)
- `POST /api/threatdemo/simulate/sqli` - SQL injection simulation
- `POST /api/threatdemo/simulate/xss` - XSS attack simulation
- `POST /api/threatdemo/simulate/ddos` - DDoS pattern simulation
- `POST /api/threatdemo/simulate/bruteforce` - Brute force simulation
- `POST /api/threatdemo/simulate/malware` - Malware detection simulation
- `POST /api/threatdemo/simulate/zeroday` - Zero-day vulnerability simulation
- `GET /api/threatdemo/active-threats` - View detected threats

## 📊 Monitoring & Analytics

### Real-Time Dashboard

The demo includes real-time threat monitoring with:

- **Live Threat Feed**: See threats as they're detected
- **Threat Statistics**: Counts by severity and type
- **User Behavior Insights**: Anomaly patterns and risk scores
- **System Health**: Performance metrics and API status

### Security Metrics

- **Detection Rate**: Percentage of threats caught
- **False Positive Rate**: Accuracy of threat detection
- **Response Time**: Speed of threat identification
- **Risk Scores**: Trending threat levels over time

## 🔒 Security Best Practices

### 1. **Threshold Configuration**
```json
{
  "ThreatDetector": {
    "ThreatThreshold": 0.7,        // Adjust based on your risk tolerance
    "AutoMitigation": false,       // Enable for automatic blocking
    "UserBehaviorSensitivity": 0.6 // Lower = more sensitive
  }
}
```

### 2. **Event Handling**
- Always log security events
- Implement graduated response (warn → block → lockdown)
- Set up alerting for critical threats
- Regular review of threat patterns

### 3. **Performance Optimization**
- Use async methods for all SDK calls
- Implement caching for frequent operations
- Monitor API response times
- Configure appropriate timeouts

## 🧪 Testing Scenarios

### SQL Injection Tests
```bash
# Test malicious search query
curl -X POST "https://localhost:5001/api/products/search" \
  -H "Content-Type: application/json" \
  -d '{"query": "'; DROP TABLE users; --"}'
```

### User Behavior Tests
```bash
# Test rapid login attempts (brute force)
for i in {1..10}; do
  curl -X POST "https://localhost:5001/api/users/login" \
    -H "Content-Type: application/json" \
    -d '{"username": "admin", "password": "wrong"}'
done
```

### Malware Upload Test
```bash
# Test suspicious file upload
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -d '{"name": "update.exe", "description": "<script>malicious</script>"}'
```

## 📈 Scaling & Production

### High-Volume Applications
- Enable SDK caching
- Use batch operations for training data
- Implement circuit breaker patterns
- Monitor SDK performance metrics

### Multi-Tenant Applications
- Set unique ApplicationId per tenant
- Isolate threat data by tenant
- Configure tenant-specific thresholds
- Implement tenant-level monitoring

### Microservices Architecture
- Deploy SDK in each service
- Centralize threat correlation
- Use service mesh for communication
- Implement distributed tracing

## 🆘 Troubleshooting

### Common Issues

1. **SDK Connection Errors**
   - Verify API endpoint is accessible
   - Check API key configuration
   - Ensure proper network connectivity

2. **High False Positives**
   - Adjust threat threshold
   - Review training data quality
   - Fine-tune detection parameters

3. **Performance Issues**
   - Enable async processing
   - Implement request caching
   - Monitor resource usage

### Debug Mode

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "ThreatDetector.SDK": "Debug"
    }
  }
}
```

## 📚 Additional Resources

- [API Documentation](../README.md)
- [SDK Reference](./sdk/README.md)
- [Architecture Guide](./docs/architecture.md)
- [Performance Tuning](./docs/performance.md)
- [Security Guidelines](./docs/security.md)

## 🤝 Support

For technical support and questions:
- GitHub Issues: [Report Issues](https://github.com/your-org/ai-threat-detector/issues)
- Documentation: [Full Docs](https://docs.ai-threat-detector.com)
- Community: [Discord Server](https://discord.gg/threat-detector)

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
