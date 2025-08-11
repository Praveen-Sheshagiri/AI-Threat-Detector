using Microsoft.EntityFrameworkCore;
using ThreatDetector.Demo.Data;

namespace ThreatDetector.Demo.Services;

public interface IUserService
{
    Task<LoginResult> LoginAsync(string username, string password, string ipAddress, string userAgent);
    Task<User?> RegisterAsync(string username, string email, string password);
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(int id, string email);
    Task RecordLoginAttemptAsync(string username, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null);
    Task<int> GetRecentFailedAttemptsAsync(string username, string ipAddress);
}

public class UserService : IUserService
{
    private readonly DemoDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(DemoDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(string username, string password, string ipAddress, string userAgent)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
            {
                await RecordLoginAttemptAsync(username, ipAddress, userAgent, false, "User not found");
                return new LoginResult { IsSuccess = false, User = null };
            }

            // In a real application, you would hash and compare passwords
            // For demo purposes, accept any password except "wrong"
            bool isPasswordValid = password != "wrong";

            if (isPasswordValid)
            {
                user.LastLoginAt = DateTime.UtcNow;
                user.LoginAttempts = 0; // Reset failed attempts on successful login
                await _context.SaveChangesAsync();

                await RecordLoginAttemptAsync(username, ipAddress, userAgent, true);
                
                _logger.LogInformation("Successful login for user {Username} from IP {IpAddress}", username, ipAddress);
                return new LoginResult { IsSuccess = true, User = user };
            }
            else
            {
                user.LoginAttempts++;
                await _context.SaveChangesAsync();

                await RecordLoginAttemptAsync(username, ipAddress, userAgent, false, "Invalid password");
                
                _logger.LogWarning("Failed login attempt for user {Username} from IP {IpAddress}", username, ipAddress);
                return new LoginResult { IsSuccess = false, User = null };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for user {Username}", username);
            return new LoginResult { IsSuccess = false, User = null };
        }
    }

    public async Task<User?> RegisterAsync(string username, string email, string password)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - username or email already exists: {Username}, {Email}", username, email);
                return null;
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LoginAttempts = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully registered new user: {Username} with email {Email}", username, email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Username}", username);
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> UpdateUserAsync(int id, string email)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            // Check if email is already used by another user
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != id);

            if (emailExists)
            {
                return false;
            }

            user.Email = email;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated email for user {UserId} to {Email}", id, email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return false;
        }
    }

    public async Task RecordLoginAttemptAsync(string username, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        try
        {
            var loginAttempt = new LoginAttempt
            {
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = isSuccessful,
                AttemptedAt = DateTime.UtcNow,
                FailureReason = failureReason ?? ""
            };

            _context.LoginAttempts.Add(loginAttempt);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording login attempt for {Username}", username);
        }
    }

    public async Task<int> GetRecentFailedAttemptsAsync(string username, string ipAddress)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-15); // Last 15 minutes

            var failedAttempts = await _context.LoginAttempts
                .CountAsync(la => la.Username == username && 
                                 la.IpAddress == ipAddress && 
                                 !la.IsSuccessful && 
                                 la.AttemptedAt >= cutoffTime);

            return failedAttempts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent failed attempts for {Username}", username);
            return 0;
        }
    }
}

public class LoginResult
{
    public bool IsSuccess { get; set; }
    public User? User { get; set; }
}
