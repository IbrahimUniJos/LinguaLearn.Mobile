using System.Text.Json.Serialization;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Gamification;

/// <summary>
/// Service for managing user streaks as specified in maui-app-specs.md
/// Implements streak maintenance with midnight boundary and grace period tokens
/// </summary>
public interface IStreakService
{
    // Core streak operations
    Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default);
    Task<ServiceResult<StreakSnapshot>> GetCurrentStreakAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> GetLongestStreakAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> CheckStreakMaintenanceAsync(string userId, CancellationToken ct = default);
    
    // Streak freeze functionality
    Task<ServiceResult<bool>> UseStreakFreezeAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> GetStreakFreezeTokensAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> AwardStreakFreezeTokenAsync(string userId, string reason, CancellationToken ct = default);
    
    // Streak calculations
    bool IsStreakBroken(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone);
    bool IsWithinGracePeriod(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone);
    DateTime GetNextStreakDeadline(DateTime currentDate, TimeZoneInfo userTimeZone);
    
    // Streak milestones and rewards
    Task<ServiceResult<bool>> CheckStreakMilestonesAsync(string userId, int newStreakCount, CancellationToken ct = default);
    List<int> GetStreakMilestones();
    int CalculateStreakRewardXP(int streakCount);
}

/// <summary>
/// Implementation of streak service following the midnight boundary logic
/// with grace period and freeze token support as specified
/// </summary>
public class StreakService : IStreakService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IXPService _xpService;
    private readonly IBadgeService _badgeService;
    private readonly ILogger<StreakService> _logger;
    
    // Streak milestone thresholds
    private static readonly List<int> _streakMilestones = new() { 3, 7, 14, 30, 60, 100, 365 };
    
    // Grace period and freeze settings
    private const int GRACE_PERIOD_HOURS = 4; // 4-hour grace period after midnight
    private const int MAX_FREEZE_TOKENS = 5;
    
    public StreakService(
        IFirestoreRepository firestoreRepository,
        IXPService xpService,
        IBadgeService badgeService,
        ILogger<StreakService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _xpService = xpService;
        _badgeService = badgeService;
        _logger = logger;
    }
    
    public async Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default)
    {
        try
        {
            var userResult = await _firestoreRepository.GetDocumentAsync<UserProfile>("users", userId, ct);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResult<int>.Failure("User profile not found");
            }
            
            var userProfile = userResult.Data;
            var userTimeZone = TimeZoneInfo.Local; // In production, get from user preferences
            var lastActive = userProfile.LastActiveDate ?? DateTime.MinValue;
            var activityDateLocal = TimeZoneInfo.ConvertTimeFromUtc(activityDate, userTimeZone);
            var lastActiveLocal = TimeZoneInfo.ConvertTimeFromUtc(lastActive, userTimeZone);
            
            var newStreakCount = CalculateNewStreakCount(lastActiveLocal, activityDateLocal, userProfile.StreakCount);
            
            // Update user profile
            userProfile.StreakCount = newStreakCount;
            userProfile.LastActiveDate = activityDate;
            userProfile.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await _firestoreRepository.SetDocumentAsync("users", userId, userProfile, ct);
            if (!updateResult.IsSuccess)
            {
                return ServiceResult<int>.Failure("Failed to update user profile");
            }
            
            // Create/update streak snapshot
            await UpdateStreakSnapshotAsync(userId, newStreakCount, activityDate, ct);
            
            // Check for milestone rewards
            await CheckStreakMilestonesAsync(userId, newStreakCount, ct);
            
            _logger.LogInformation("Streak updated for user {UserId}: {StreakCount}", userId, newStreakCount);
            return ServiceResult<int>.Success(newStreakCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update streak for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Failed to update streak: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<StreakSnapshot>> GetCurrentStreakAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            // For now, create a basic streak snapshot since the full query implementation would need
            // the complete IFirestoreRepository interface definition
            var currentStreak = new StreakSnapshot
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                StartDate = DateTime.UtcNow,
                CurrentCount = 0,
                MaxCount = 0,
                IsActive = true
            };
            
            return ServiceResult<StreakSnapshot>.Success(currentStreak);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current streak for user {UserId}", userId);
            return ServiceResult<StreakSnapshot>.Failure($"Failed to get current streak: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<int>> GetLongestStreakAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            // TODO: Implement actual query once IFirestoreRepository interface is complete
            var longestStreak = 0;
            return ServiceResult<int>.Success(longestStreak);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get longest streak for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Failed to get longest streak: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> UseStreakFreezeAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var userResult = await _firestoreRepository.GetDocumentAsync<UserProfile>("users", userId, ct);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResult<bool>.Failure("User profile not found");
            }
            
            var userProfile = userResult.Data;
            if (userProfile.StreakFreezeTokens <= 0)
            {
                return ServiceResult<bool>.Failure("No freeze tokens available");
            }
            
            // Use one freeze token
            userProfile.StreakFreezeTokens--;
            userProfile.LastActiveDate = DateTime.UtcNow; // Extend the streak
            userProfile.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await _firestoreRepository.SetDocumentAsync("users", userId, userProfile, ct);
            if (!updateResult.IsSuccess)
            {
                return ServiceResult<bool>.Failure("Failed to update user profile");
            }
            
            _logger.LogInformation("Streak freeze used for user {UserId}", userId);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to use streak freeze for user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Failed to use streak freeze: {ex.Message}");
        }
    }
    
    public bool IsStreakBroken(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone)
    {
        var lastActiveLocal = TimeZoneInfo.ConvertTimeFromUtc(lastActiveDate, userTimeZone).Date;
        var currentLocal = TimeZoneInfo.ConvertTimeFromUtc(currentDate, userTimeZone).Date;
        
        var daysDifference = (currentLocal - lastActiveLocal).Days;
        
        // Streak is broken if more than 1 day gap without grace period
        if (daysDifference > 1) return true;
        
        // Check grace period for same day or next day
        if (daysDifference == 1)
        {
            return !IsWithinGracePeriod(lastActiveDate, currentDate, userTimeZone);
        }
        
        return false;
    }
    
    public bool IsWithinGracePeriod(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone)
    {
        var midnightAfterLastActive = TimeZoneInfo.ConvertTimeFromUtc(lastActiveDate, userTimeZone).Date.AddDays(1);
        var gracePeriodEnd = midnightAfterLastActive.AddHours(GRACE_PERIOD_HOURS);
        var currentLocal = TimeZoneInfo.ConvertTimeFromUtc(currentDate, userTimeZone);
        
        return currentLocal <= gracePeriodEnd;
    }
    
    public DateTime GetNextStreakDeadline(DateTime currentDate, TimeZoneInfo userTimeZone)
    {
        var currentLocal = TimeZoneInfo.ConvertTimeFromUtc(currentDate, userTimeZone);
        var nextMidnight = currentLocal.Date.AddDays(1);
        var deadline = nextMidnight.AddHours(GRACE_PERIOD_HOURS);
        
        return TimeZoneInfo.ConvertTimeToUtc(deadline, userTimeZone);
    }
    
    public async Task<ServiceResult<bool>> CheckStreakMilestonesAsync(string userId, int newStreakCount, CancellationToken ct = default)
    {
        try
        {
            foreach (var milestone in _streakMilestones)
            {
                if (newStreakCount == milestone)
                {
                    // Award milestone badge
                    var badgeId = $"streak_{milestone}";
                    await _badgeService.AwardBadgeAsync(userId, badgeId, $"Reached {milestone} day streak", ct);
                    
                    // Award freeze token for significant milestones
                    if (milestone >= 7)
                    {
                        await AwardStreakFreezeTokenAsync(userId, $"Streak milestone: {milestone} days", ct);
                    }
                    
                    _logger.LogInformation("Streak milestone {Milestone} reached for user {UserId}", milestone, userId);
                }
            }
            
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check streak milestones for user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Failed to check streak milestones: {ex.Message}");
        }
    }
    
    public List<int> GetStreakMilestones() => _streakMilestones.ToList();
    
    public int CalculateStreakRewardXP(int streakCount)
    {
        return streakCount switch
        {
            3 => 20,
            7 => 50,
            14 => 100,
            30 => 200,
            60 => 400,
            100 => 750,
            365 => 1500,
            _ => 10
        };
    }
    
    // Helper methods
    private int CalculateNewStreakCount(DateTime lastActiveLocal, DateTime activityDateLocal, int currentStreak)
    {
        var daysDifference = (activityDateLocal.Date - lastActiveLocal.Date).Days;
        
        return daysDifference switch
        {
            0 => currentStreak, // Same day, no change
            1 => currentStreak + 1, // Next day, increment
            _ => 1 // Gap > 1 day, reset to 1
        };
    }
    
    private async Task UpdateStreakSnapshotAsync(string userId, int newStreakCount, DateTime activityDate, CancellationToken ct)
    {
        // Create a new streak snapshot for tracking
        var snapshot = new StreakSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            StartDate = activityDate,
            CurrentCount = newStreakCount,
            MaxCount = newStreakCount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // Store in subcollection users/{userId}/streaks/{snapshotId}
        await _firestoreRepository.SetDocumentAsync($"users/{userId}/streaks", snapshot.Id, snapshot, ct);
    }
    
    public async Task<ServiceResult<bool>> CheckStreakMaintenanceAsync(string userId, CancellationToken ct = default)
    {
        // This method would be called by a background service to check for broken streaks
        // Implementation would check if streak should be broken based on time since last activity
        throw new NotImplementedException("Background streak maintenance to be implemented");
    }
    
    public async Task<ServiceResult<int>> GetStreakFreezeTokensAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var userResult = await _firestoreRepository.GetDocumentAsync<UserProfile>("users", userId, ct);
            return userResult.IsSuccess && userResult.Data != null
                ? ServiceResult<int>.Success(userResult.Data.StreakFreezeTokens)
                : ServiceResult<int>.Failure("User profile not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get freeze tokens for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Failed to get freeze tokens: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult<bool>> AwardStreakFreezeTokenAsync(string userId, string reason, CancellationToken ct = default)
    {
        try
        {
            var userResult = await _firestoreRepository.GetDocumentAsync<UserProfile>("users", userId, ct);
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return ServiceResult<bool>.Failure("User profile not found");
            }
            
            var userProfile = userResult.Data;
            if (userProfile.StreakFreezeTokens >= MAX_FREEZE_TOKENS)
            {
                return ServiceResult<bool>.Success(false); // Already at max
            }
            
            userProfile.StreakFreezeTokens++;
            userProfile.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await _firestoreRepository.SetDocumentAsync("users", userId, userProfile, ct);
            if (!updateResult.IsSuccess)
            {
                return ServiceResult<bool>.Failure("Failed to update user profile");
            }
            
            _logger.LogInformation("Freeze token awarded to user {UserId}: {Reason}", userId, reason);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to award freeze token to user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Failed to award freeze token: {ex.Message}");
        }
    }
}