using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Activity;

/// <summary>
/// Service for tracking and managing user activities
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// Records a new activity for the user
    /// </summary>
    Task<ServiceResult<bool>> RecordActivityAsync(string userId, ActivityItem activity, CancellationToken ct = default);
    
    /// <summary>
    /// Gets recent activities for a user
    /// </summary>
    Task<ServiceResult<List<ActivityItem>>> GetRecentActivitiesAsync(string userId, int limit = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Records a lesson completion activity
    /// </summary>
    Task<ServiceResult<bool>> RecordLessonCompletedAsync(string userId, string lessonTitle, int xpEarned, CancellationToken ct = default);
    
    /// <summary>
    /// Records a badge earned activity
    /// </summary>
    Task<ServiceResult<bool>> RecordBadgeEarnedAsync(string userId, string badgeTitle, CancellationToken ct = default);
    
    /// <summary>
    /// Records a streak milestone activity
    /// </summary>
    Task<ServiceResult<bool>> RecordStreakMilestoneAsync(string userId, int streakCount, CancellationToken ct = default);
    
    /// <summary>
    /// Records a level up activity
    /// </summary>
    Task<ServiceResult<bool>> RecordLevelUpAsync(string userId, int newLevel, CancellationToken ct = default);
    
    /// <summary>
    /// Records a quiz completion activity
    /// </summary>
    Task<ServiceResult<bool>> RecordQuizCompletedAsync(string userId, string quizTitle, double accuracy, int xpEarned, CancellationToken ct = default);
    
    /// <summary>
    /// Clears old activities (for maintenance)
    /// </summary>
    Task<ServiceResult<bool>> ClearOldActivitiesAsync(string userId, DateTime olderThan, CancellationToken ct = default);
}