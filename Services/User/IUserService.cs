using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.User;

/// <summary>
/// Service for managing user profile, preferences, and gamification data
/// Implements the user management layer as specified in maui-app-specs.md
/// </summary>
public interface IUserService
{
    // Profile Management
    Task<ServiceResult<UserProfile>> GetUserProfileAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<UserProfile>> CreateUserProfileAsync(UserProfile profile, CancellationToken ct = default);
    Task<ServiceResult<UserProfile>> UpdateUserProfileAsync(UserProfile profile, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteUserProfileAsync(string userId, CancellationToken ct = default);
    
    // Onboarding
    Task<ServiceResult<bool>> CompleteOnboardingAsync(string userId, string nativeLanguage, string targetLanguage, CancellationToken ct = default);
    Task<ServiceResult<bool>> HasCompletedOnboardingAsync(string userId, CancellationToken ct = default);
    
    // Preferences
    Task<ServiceResult<UserPreferences>> GetUserPreferencesAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<UserPreferences>> UpdateUserPreferencesAsync(string userId, UserPreferences preferences, CancellationToken ct = default);
    
    // Gamification - XP & Levels
    Task<ServiceResult<int>> AddXPAsync(string userId, int xpAmount, string reason, CancellationToken ct = default);
    Task<ServiceResult<int>> GetCurrentLevelAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> GetXPForNextLevelAsync(string userId, CancellationToken ct = default);
    
    // Streaks
    Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default);
    Task<ServiceResult<bool>> UseStreakFreezeTokenAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<StreakSnapshot>> GetCurrentStreakAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> GetLongestStreakAsync(string userId, CancellationToken ct = default);
    
    // Badges
    Task<ServiceResult<bool>> AwardBadgeAsync(string userId, string badgeId, CancellationToken ct = default);
    Task<ServiceResult<List<UserBadge>>> GetUserBadgesAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> HasBadgeAsync(string userId, string badgeId, CancellationToken ct = default);
    
    // Statistics
    Task<ServiceResult<UserStats>> GetUserStatsAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<UserStats>> UpdateUserStatsAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<WeeklyProgress>> GetWeeklyProgressAsync(string userId, CancellationToken ct = default);
    
    // Utility Methods
    Task<ServiceResult<bool>> IsActiveUserAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<DateTime?>> GetLastActiveAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> MarkUserActiveAsync(string userId, CancellationToken ct = default);
    
    // Language Support
    Task<ServiceResult<List<LanguageOption>>> GetAvailableLanguagesAsync(CancellationToken ct = default);
    Task<ServiceResult<bool>> SetLanguagePreferencesAsync(string userId, string nativeLanguage, string targetLanguage, CancellationToken ct = default);
}