using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.User;

/// <summary>
/// Comprehensive user service implementing user profile management, gamification, 
/// and onboarding functionality as specified in maui-app-specs.md
/// </summary>
public class UserService : IUserService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<UserService> _logger;
    
    private const string UsersCollection = "users";
    
    // Level calculation constants (from specs: Level XP = 50 * level^1.7)
    private const double LevelBaseXP = 50.0;
    private const double LevelExponent = 1.7;

    public UserService(
        IFirestoreRepository firestoreRepository,
        IFirebaseAuthService authService,
        ILogger<UserService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _authService = authService;
        _logger = logger;
    }

    #region Profile Management

    public async Task<ServiceResult<UserProfile>> GetUserProfileAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting user profile for user: {UserId}", userId);
            
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess)
            {
                return ServiceResult.Failure<UserProfile>("Failed to retrieve user profile");
            }

            if (result.Data == null)
            {
                return ServiceResult.Failure<UserProfile>("User profile not found");
            }

            return ServiceResult.Success(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile for user: {UserId}", userId);
            return ServiceResult.Failure<UserProfile>("Failed to retrieve user profile", ex);
        }
    }

    public async Task<ServiceResult<UserProfile>> CreateUserProfileAsync(UserProfile profile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating user profile for user: {UserId}", profile.Id);

            // Ensure profile has proper defaults and metadata
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version = 1;
            profile.Level = CalculateLevel(profile.XP);

            var result = await _firestoreRepository.SetDocumentAsync(UsersCollection, profile.Id, profile, ct);
            
            if (!result.IsSuccess)
            {
                return ServiceResult.Failure<UserProfile>("Failed to create user profile");
            }
            
            _logger.LogInformation("User profile created successfully for user: {UserId}", profile.Id);
            return ServiceResult.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user profile for user: {UserId}", profile.Id);
            return ServiceResult.Failure<UserProfile>("Failed to create user profile", ex);
        }
    }

    public async Task<ServiceResult<UserProfile>> UpdateUserProfileAsync(UserProfile profile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating user profile for user: {UserId}", profile.Id);

            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;
            profile.Level = CalculateLevel(profile.XP);

            var result = await _firestoreRepository.SetDocumentAsync(UsersCollection, profile.Id, profile, ct);
            
            if (!result.IsSuccess)
            {
                return ServiceResult.Failure<UserProfile>("Failed to update user profile");
            }
            
            _logger.LogInformation("User profile updated successfully for user: {UserId}", profile.Id);
            return ServiceResult.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile for user: {UserId}", profile.Id);
            return ServiceResult.Failure<UserProfile>("Failed to update user profile", ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteUserProfileAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting user profile for user: {UserId}", userId);

            var result = await _firestoreRepository.DeleteDocumentAsync(UsersCollection, userId, ct);
            
            if (!result.IsSuccess)
            {
                return ServiceResult.Failure<bool>("Failed to delete user profile");
            }
            
            _logger.LogInformation("User profile deleted successfully for user: {UserId}", userId);
            return ServiceResult.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user profile for user: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to delete user profile", ex);
        }
    }

    #endregion

    #region Onboarding

    public async Task<ServiceResult<bool>> CompleteOnboardingAsync(
        string userId, 
        string nativeLanguage, 
        string targetLanguage, 
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Completing onboarding for user: {UserId}", userId);

            // Get current user session for additional info
            var session = await _authService.GetCurrentSessionAsync();
            if (session == null || session.UserId != userId)
            {
                return ServiceResult.Failure<bool>("Invalid user session");
            }

            // Check if profile already exists
            var existingResult = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            UserProfile profile;
            if (existingResult.IsSuccess && existingResult.Data != null)
            {
                // Update existing profile
                profile = existingResult.Data;
                profile.NativeLanguage = nativeLanguage;
                profile.TargetLanguage = targetLanguage;
                profile.HasCompletedOnboarding = true;
                profile.UpdatedAt = DateTime.UtcNow;
                profile.Version++;
                
                var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
                if (!updateResult.IsSuccess)
                {
                    return ServiceResult.Failure<bool>("Failed to update user profile during onboarding");
                }
            }
            else
            {
                // Create new profile
                profile = new UserProfile
                {
                    Id = userId,
                    Email = session.Email,
                    DisplayName = session.DisplayName ?? "User",
                    NativeLanguage = nativeLanguage,
                    TargetLanguage = targetLanguage,
                    HasCompletedOnboarding = true,
                    XP = 0,
                    Level = 1,
                    StreakCount = 0,
                    StreakFreezeTokens = 3, // Give new users 3 freeze tokens
                    Preferences = new UserPreferences(), // Use defaults
                    Badges = new List<UserBadge>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1
                };

                var createResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
                if (!createResult.IsSuccess)
                {
                    return ServiceResult.Failure<bool>("Failed to create user profile during onboarding");
                }
            }

            // Award onboarding completion badge
            await AwardBadgeAsync(userId, "onboarding_complete", ct);
            
            _logger.LogInformation("Onboarding completed successfully for user: {UserId}", userId);
            return ServiceResult.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete onboarding for user: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to complete onboarding", ex);
        }
    }

    public async Task<ServiceResult<bool>> HasCompletedOnboardingAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Success(false);
            }
            
            return ServiceResult.Success(result.Data.HasCompletedOnboarding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check onboarding status for user: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to check onboarding status", ex);
        }
    }

    #endregion

    #region Preferences

    public async Task<ServiceResult<UserPreferences>> GetUserPreferencesAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<UserPreferences>("User profile not found");
            }

            return ServiceResult.Success(result.Data.Preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user preferences for user: {UserId}", userId);
            return ServiceResult.Failure<UserPreferences>("Failed to get user preferences", ex);
        }
    }

    public async Task<ServiceResult<UserPreferences>> UpdateUserPreferencesAsync(
        string userId, 
        UserPreferences preferences, 
        CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<UserPreferences>("User profile not found");
            }

            var profile = result.Data;
            profile.Preferences = preferences;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<UserPreferences>("Failed to update user preferences");
            }
            
            return ServiceResult.Success(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user preferences for user: {UserId}", userId);
            return ServiceResult.Failure<UserPreferences>("Failed to update user preferences", ex);
        }
    }

    #endregion

    #region Gamification - XP & Levels

    public async Task<ServiceResult<int>> AddXPAsync(string userId, int xpAmount, string reason, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Adding {XP} XP to user: {UserId} for reason: {Reason}", xpAmount, userId, reason);

            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<int>("User profile not found");
            }

            var profile = result.Data;
            var oldLevel = profile.Level;
            profile.XP += xpAmount;
            profile.Level = CalculateLevel(profile.XP);
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<int>("Failed to update XP");
            }

            // Check for level up badge
            if (profile.Level > oldLevel)
            {
                await AwardBadgeAsync(userId, $"level_{profile.Level}", ct);
                _logger.LogInformation("User {UserId} leveled up to level {Level}", userId, profile.Level);
            }

            return ServiceResult.Success(profile.XP);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add XP for user: {UserId}", userId);
            return ServiceResult.Failure<int>("Failed to add XP", ex);
        }
    }

    public async Task<ServiceResult<int>> GetCurrentLevelAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<int>("User profile not found");
            }

            return ServiceResult.Success(result.Data.Level);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current level for user: {UserId}", userId);
            return ServiceResult.Failure<int>("Failed to get current level", ex);
        }
    }

    public async Task<ServiceResult<int>> GetXPForNextLevelAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<int>("User profile not found");
            }

            var profile = result.Data;
            var nextLevelXP = CalculateXPForLevel(profile.Level + 1);
            var xpNeeded = nextLevelXP - profile.XP;

            return ServiceResult.Success(Math.Max(0, xpNeeded));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get XP for next level for user: {UserId}", userId);
            return ServiceResult.Failure<int>("Failed to get XP for next level", ex);
        }
    }

    #endregion

    #region Streaks

    public async Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<int>("User profile not found");
            }

            var profile = result.Data;
            var today = activityDate.Date;
            var lastActive = profile.LastActiveDate?.Date;

            var oldStreak = profile.StreakCount;

            if (lastActive == null)
            {
                // First activity
                profile.StreakCount = 1;
            }
            else if (lastActive == today)
            {
                // Same day, don't change streak
                return ServiceResult.Success(profile.StreakCount);
            }
            else if (lastActive == today.AddDays(-1))
            {
                // Consecutive day, extend streak
                profile.StreakCount++;
            }
            else
            {
                // Streak broken, start new one
                profile.StreakCount = 1;
            }

            profile.LastActiveDate = activityDate;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<int>("Failed to update streak");
            }

            // Award streak badges
            if (profile.StreakCount > oldStreak && profile.StreakCount % 7 == 0)
            {
                await AwardBadgeAsync(userId, $"streak_{profile.StreakCount}", ct);
            }

            return ServiceResult.Success(profile.StreakCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update streak for user: {UserId}", userId);
            return ServiceResult.Failure<int>("Failed to update streak", ex);
        }
    }

    public async Task<ServiceResult<bool>> UseStreakFreezeTokenAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<bool>("User profile not found");
            }

            var profile = result.Data;

            if (profile.StreakFreezeTokens <= 0)
            {
                return ServiceResult.Failure<bool>("No streak freeze tokens available");
            }

            profile.StreakFreezeTokens--;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<bool>("Failed to use streak freeze token");
            }

            return ServiceResult.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to use streak freeze token for user: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to use streak freeze token", ex);
        }
    }

    public async Task<ServiceResult<StreakSnapshot>> GetCurrentStreakAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<StreakSnapshot>("User profile not found");
            }

            var profile = result.Data;
            var snapshot = new StreakSnapshot
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                StartDate = profile.LastActiveDate?.AddDays(-(profile.StreakCount - 1)) ?? DateTime.UtcNow,
                CurrentCount = profile.StreakCount,
                MaxCount = profile.StreakCount,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return ServiceResult.Success(snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current streak for user: {UserId}", userId);
            return ServiceResult.Failure<StreakSnapshot>("Failed to get current streak", ex);
        }
    }

    public async Task<ServiceResult<int>> GetLongestStreakAsync(string userId, CancellationToken ct = default)
    {
        // For now, return current streak. In the future, we could track historical streaks
        var currentStreakResult = await GetCurrentStreakAsync(userId, ct);
        if (!currentStreakResult.IsSuccess)
        {
            return ServiceResult.Failure<int>(currentStreakResult.ErrorMessage!);
        }

        return ServiceResult.Success(currentStreakResult.Data!.CurrentCount);
    }

    #endregion

    #region Badges

    public async Task<ServiceResult<bool>> AwardBadgeAsync(string userId, string badgeId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<bool>("User profile not found");
            }

            var profile = result.Data;

            // Check if user already has this badge
            if (profile.Badges.Any(b => b.BadgeId == badgeId))
            {
                return ServiceResult.Success(false); // Already has badge
            }

            var badge = new UserBadge
            {
                BadgeId = badgeId,
                EarnedAt = DateTime.UtcNow
            };

            profile.Badges.Add(badge);
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<bool>("Failed to award badge");
            }

            _logger.LogInformation("Awarded badge {BadgeId} to user: {UserId}", badgeId, userId);
            return ServiceResult.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to award badge {BadgeId} to user: {UserId}", badgeId, userId);
            return ServiceResult.Failure<bool>("Failed to award badge", ex);
        }
    }

    public async Task<ServiceResult<List<UserBadge>>> GetUserBadgesAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<List<UserBadge>>("User profile not found");
            }

            return ServiceResult.Success(result.Data.Badges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user badges for user: {UserId}", userId);
            return ServiceResult.Failure<List<UserBadge>>("Failed to get user badges", ex);
        }
    }

    public async Task<ServiceResult<bool>> HasBadgeAsync(string userId, string badgeId, CancellationToken ct = default)
    {
        try
        {
            var badges = await GetUserBadgesAsync(userId, ct);
            if (!badges.IsSuccess)
            {
                return ServiceResult.Failure<bool>(badges.ErrorMessage!);
            }

            var hasBadge = badges.Data!.Any(b => b.BadgeId == badgeId);
            return ServiceResult.Success(hasBadge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check badge {BadgeId} for user: {UserId}", badgeId, userId);
            return ServiceResult.Failure<bool>("Failed to check badge", ex);
        }
    }

    #endregion

    #region Statistics (Placeholder implementations)

    public async Task<ServiceResult<UserStats>> GetUserStatsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<UserStats>("User profile not found");
            }

            var profile = result.Data;
            // Create basic stats from profile
            var stats = new UserStats
            {
                UserId = userId,
                TotalXPEarned = profile.XP,
                BadgesEarned = profile.Badges.Count,
                LongestStreak = profile.StreakCount,
                LastUpdated = DateTime.UtcNow
            };

            return ServiceResult.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user stats for user: {UserId}", userId);
            return ServiceResult.Failure<UserStats>("Failed to get user stats", ex);
        }
    }

    public async Task<ServiceResult<UserStats>> UpdateUserStatsAsync(string userId, CancellationToken ct = default)
    {
        // Placeholder - in full implementation would recalculate from progress records
        return await GetUserStatsAsync(userId, ct);
    }

    public async Task<ServiceResult<WeeklyProgress>> GetWeeklyProgressAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<WeeklyProgress>("User profile not found");
            }

            var profile = result.Data;
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var progress = new WeeklyProgress
            {
                WeekStartDate = weekStart,
                Goal = profile.Preferences.WeeklyGoal,
                LessonsCompleted = 0, // Would be calculated from progress records
                XPEarned = 0,
                DailyActivity = new Dictionary<string, bool>()
            };

            return ServiceResult.Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weekly progress for user: {UserId}", userId);
            return ServiceResult.Failure<WeeklyProgress>("Failed to get weekly progress", ex);
        }
    }

    #endregion

    #region Utility Methods

    public async Task<ServiceResult<bool>> IsActiveUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Success(false);
            }

            var profile = result.Data;
            var lastActive = profile.LastActiveDate;
            var isActive = lastActive.HasValue && 
                          lastActive.Value.Date >= DateTime.UtcNow.Date.AddDays(-7);

            return ServiceResult.Success(isActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user is active: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to check user activity", ex);
        }
    }

    public async Task<ServiceResult<DateTime?>> GetLastActiveAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Success<DateTime?>(null);
            }
            
            return ServiceResult.Success<DateTime?>(result.Data.LastActiveDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get last active date for user: {UserId}", userId);
            return ServiceResult.Failure<DateTime?>("Failed to get last active date", ex);
        }
    }

    public async Task<ServiceResult<bool>> MarkUserActiveAsync(string userId, CancellationToken ct = default)
    {
        var streakResult = await UpdateStreakAsync(userId, DateTime.UtcNow, ct);
        return ServiceResult.Success(streakResult.IsSuccess);
    }

    #endregion

    #region Language Support

    public async Task<ServiceResult<List<LanguageOption>>> GetAvailableLanguagesAsync(CancellationToken ct = default)
    {
        try
        {
            // Return static language list from UserModels
            await Task.CompletedTask; // Simulate async operation
            return ServiceResult.Success(AvailableLanguages.All.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available languages");
            return ServiceResult.Failure<List<LanguageOption>>("Failed to get available languages", ex);
        }
    }

    public async Task<ServiceResult<bool>> SetLanguagePreferencesAsync(
        string userId, 
        string nativeLanguage, 
        string targetLanguage, 
        CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetDocumentAsync<UserProfile>(UsersCollection, userId, ct);
            
            if (!result.IsSuccess || result.Data == null)
            {
                return ServiceResult.Failure<bool>("User profile not found");
            }

            var profile = result.Data;
            profile.NativeLanguage = nativeLanguage;
            profile.TargetLanguage = targetLanguage;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.Version++;

            var updateResult = await _firestoreRepository.SetDocumentAsync(UsersCollection, userId, profile, ct);
            
            if (!updateResult.IsSuccess)
            {
                return ServiceResult.Failure<bool>("Failed to set language preferences");
            }

            return ServiceResult.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set language preferences for user: {UserId}", userId);
            return ServiceResult.Failure<bool>("Failed to set language preferences", ex);
        }
    }

    #endregion

    #region Current User

    public async Task<string?> GetCurrentUserIdAsync(CancellationToken ct = default)
    {
        try
        {
            var session = await _authService.GetCurrentSessionAsync();
            return session?.UserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user ID");
            return null;
        }
    }

    #endregion

    #region Private Helper Methods

    private int CalculateLevel(int xp)
    {
        if (xp <= 0) return 1;
        
        // Level XP = 50 * level^1.7, so level = (XP / 50)^(1/1.7)
        var level = Math.Pow(xp / LevelBaseXP, 1.0 / LevelExponent);
        return Math.Max(1, (int)Math.Floor(level));
    }

    private int CalculateXPForLevel(int level)
    {
        if (level <= 1) return 0;
        
        // Level XP = 50 * level^1.7
        var xp = LevelBaseXP * Math.Pow(level, LevelExponent);
        return (int)Math.Ceiling(xp);
    }

    #endregion
}