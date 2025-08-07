using Microsoft.AspNetCore.Mvc;
using ThreatDetector.Demo.Services;
using ThreatDetector.Demo.Data;
using ThreatDetector.SDK;

namespace ThreatDetector.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IThreatDetectorClient _threatDetectorClient;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IThreatDetectorClient threatDetectorClient,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _threatDetectorClient = threatDetectorClient;
        _logger = logger;
    }

    /// <summary>
    /// User login with behavior analysis
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var userAgent = Request.Headers.UserAgent.ToString();

            // Analyze login attempt for threats
            var loginData = new
            {
                username = request.Username,
                ipAddress = ipAddress,
                userAgent = userAgent,
                timestamp = DateTime.UtcNow,
                action = "UserLogin",
                hasRememberMe = request.RememberMe,
                sessionId = HttpContext.Session.Id
            };

            // Check for suspicious login patterns
            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(loginData, "UserLogin");
            
            if (threatResult.IsThreat && threatResult.ThreatScore > 0.8)
            {
                _logger.LogWarning("Suspicious login attempt for user {Username} from IP {IpAddress}", 
                    request.Username, ipAddress);
                
                await _userService.RecordLoginAttemptAsync(request.Username, ipAddress, userAgent, false, "Blocked due to suspicious activity");
                
                return BadRequest(new { 
                    error = "Login attempt blocked due to security concerns", 
                    threatId = threatResult.ThreatId 
                });
            }

            // Attempt login
            var loginResult = await _userService.LoginAsync(request.Username, request.Password, ipAddress, userAgent);
            
            if (loginResult.IsSuccess)
            {
                // Analyze user behavior after successful login
                var behaviorData = new
                {
                    eventType = "SuccessfulLogin",
                    ipAddress = ipAddress,
                    userAgent = userAgent,
                    loginTime = DateTime.UtcNow,
                    previousLoginTime = loginResult.User?.LastLoginAt,
                    loginMethod = "Password",
                    sessionId = HttpContext.Session.Id,
                    isNewDevice = IsNewDevice(userAgent, loginResult.User?.Id.ToString() ?? "")
                };

                var behaviorResult = await _threatDetectorClient.AnalyzeUserBehaviorAsync(
                    loginResult.User?.Id.ToString() ?? "", behaviorData);
                
                if (behaviorResult.IsAnomalous)
                {
                    _logger.LogWarning("Anomalous login behavior detected for user {UserId}. Score: {AnomalyScore}", 
                        loginResult.User?.Id, behaviorResult.AnomalyScore);
                    
                    // Add warning to response but allow login
                    return Ok(new { 
                        message = "Login successful", 
                        user = new { 
                            id = loginResult.User?.Id, 
                            username = loginResult.User?.Username, 
                            email = loginResult.User?.Email,
                            role = loginResult.User?.Role 
                        },
                        warning = "Unusual login behavior detected. Please verify your identity.",
                        anomalyScore = behaviorResult.AnomalyScore
                    });
                }

                return Ok(new { 
                    message = "Login successful", 
                    user = new { 
                        id = loginResult.User?.Id, 
                        username = loginResult.User?.Username, 
                        email = loginResult.User?.Email,
                        role = loginResult.User?.Role 
                    }
                });
            }
            else
            {
                // Analyze failed login patterns
                var failedAttempts = await _userService.GetRecentFailedAttemptsAsync(request.Username, ipAddress);
                
                if (failedAttempts >= 3)
                {
                    var bruteForceData = new
                    {
                        username = request.Username,
                        ipAddress = ipAddress,
                        failedAttempts = failedAttempts,
                        action = "PotentialBruteForce",
                        timeWindow = TimeSpan.FromMinutes(15).TotalMinutes
                    };

                    var bruteForceResult = await _threatDetectorClient.AnalyzeThreatAsync(bruteForceData, "BruteForceAttack");
                    
                    if (bruteForceResult.IsThreat)
                    {
                        _logger.LogError("Potential brute force attack detected for user {Username} from IP {IpAddress}", 
                            request.Username, ipAddress);
                        
                        return StatusCode(429, new { 
                            error = "Too many failed login attempts. Account temporarily locked.",
                            threatId = bruteForceResult.ThreatId
                        });
                    }
                }

                return Unauthorized(new { error = "Invalid username or password" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Register new user with validation
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            
            // Analyze registration data for threats
            var registrationData = new
            {
                username = request.Username,
                email = request.Email,
                ipAddress = ipAddress,
                userAgent = Request.Headers.UserAgent.ToString(),
                timestamp = DateTime.UtcNow,
                action = "UserRegistration",
                usernameLength = request.Username?.Length ?? 0,
                emailDomain = request.Email?.Split('@').LastOrDefault() ?? "",
                containsSuspiciousChars = ContainsSuspiciousCharacters(request.Username, request.Email)
            };

            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(registrationData, "UserRegistration");
            
            if (threatResult.IsThreat && threatResult.ThreatScore > 0.7)
            {
                _logger.LogWarning("Suspicious registration attempt blocked. Username: {Username}, Email: {Email}", 
                    request.Username, request.Email);
                
                return BadRequest(new { 
                    error = "Registration blocked due to security concerns", 
                    threatId = threatResult.ThreatId 
                });
            }

            var user = await _userService.RegisterAsync(request.Username, request.Email, request.Password);
            
            if (user != null)
            {
                return Ok(new { 
                    message = "User registered successfully", 
                    userId = user.Id,
                    username = user.Username
                });
            }
            else
            {
                return BadRequest(new { error = "Username or email already exists" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user profile with activity tracking
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUserProfile(int userId)
    {
        try
        {
            // Track user profile access
            var accessData = new
            {
                accessedUserId = userId,
                accessorIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent = Request.Headers.UserAgent.ToString(),
                timestamp = DateTime.UtcNow,
                action = "ProfileAccess",
                sessionId = HttpContext.Session.Id
            };

            // Analyze for potential unauthorized access
            var behaviorResult = await _threatDetectorClient.AnalyzeUserBehaviorAsync(
                userId.ToString(), accessData);
            
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var response = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt,
                lastLoginAt = user.LastLoginAt,
                isActive = user.IsActive
            };

            if (behaviorResult.IsAnomalous)
            {
                _logger.LogWarning("Anomalous profile access for user {UserId}. Score: {AnomalyScore}", 
                    userId, behaviorResult.AnomalyScore);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<ActionResult> UpdateUserProfile(int userId, [FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Analyze profile update for threats
            var updateData = new
            {
                userId = userId,
                newEmail = request.Email,
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent = Request.Headers.UserAgent.ToString(),
                timestamp = DateTime.UtcNow,
                action = "ProfileUpdate",
                changedFields = GetChangedFields(request)
            };

            var threatResult = await _threatDetectorClient.AnalyzeThreatAsync(updateData, "ProfileUpdate");
            
            if (threatResult.IsThreat && threatResult.ThreatScore > 0.8)
            {
                return BadRequest(new { 
                    error = "Profile update blocked due to security concerns", 
                    threatId = threatResult.ThreatId 
                });
            }

            var success = await _userService.UpdateUserAsync(userId, request.Email);
            
            if (success)
            {
                return Ok(new { message = "Profile updated successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to update profile" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private bool IsNewDevice(string userAgent, string userId)
    {
        // Simplified device detection - in real implementation, would use more sophisticated fingerprinting
        var deviceFingerprint = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userAgent));
        
        // Store and check device fingerprints in session or database
        var knownDevices = HttpContext.Session.GetString($"devices_{userId}")?.Split(',') ?? Array.Empty<string>();
        
        if (!knownDevices.Contains(deviceFingerprint))
        {
            var updatedDevices = knownDevices.Append(deviceFingerprint).ToArray();
            HttpContext.Session.SetString($"devices_{userId}", string.Join(',', updatedDevices));
            return true;
        }
        
        return false;
    }

    private static bool ContainsSuspiciousCharacters(string? username, string? email)
    {
        var suspiciousPatterns = new[] { "<script", "javascript:", "eval(", "exec(", "drop table", "union select" };
        var text = $"{username} {email}".ToLower();
        
        return suspiciousPatterns.Any(pattern => text.Contains(pattern));
    }

    private static List<string> GetChangedFields(UpdateProfileRequest request)
    {
        var fields = new List<string>();
        
        if (!string.IsNullOrEmpty(request.Email))
            fields.Add("email");
            
        return fields;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string Email { get; set; } = string.Empty;
}
