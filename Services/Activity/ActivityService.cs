using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Activity;

public class ActivityService : IActivityService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(IFirestoreRepository firestoreRepository, ILogger<ActivityService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> RecordActivityAsync(string userId, ActivityItem activity, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<bool>.Failure("User ID is required");

            if (activity == null)
                return ServiceResult<bool>.Failure("Activity is required");

            activity.UserId = userId;
            activity.Timestamp = DateTime.UtcNow;

            if (string.IsNullOrEmpty(activity.Id))
                activity.Id = Guid.NewGuid().ToString();

            var result = await _firestoreRepository.SetDocumentAsync(
                $"users/{userId}/activities", 
                activity.Id, 
                activity, 
                ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Activity recorded for user {UserId}: {ActivityType}", userId, activity.Type);
                return ServiceResult<bool>.Success(true);
            }

            return ServiceResult<bool>.Failure(result.ErrorMessage ?? "Failed to record activity");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording activity for user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Error recording activity: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ActivityItem>>> GetRecentActivitiesAsync(string userId, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<List<ActivityItem>>.Failure("User ID is required");

            var result = await _firestoreRepository.GetCollectionAsync<ActivityItem>($"users/{userId}/activities", ct);

            if (result.IsSuccess && result.Data != null)
            {
                var recentActivities = result.Data
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .ToList();

                return ServiceResult<List<ActivityItem>>.Success(recentActivities);
            }

            return ServiceResult<List<ActivityItem>>.Failure(result.ErrorMessage ?? "Failed to get activities");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
            return ServiceResult<List<ActivityItem>>.Failure($"Error getting activities: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RecordLessonCompletedAsync(string userId, string lessonTitle, int xpEarned, CancellationToken ct = default)
    {
        var activity = ActivityHelper.CreateLessonCompletedActivity(userId, lessonTitle, xpEarned);
        return await RecordActivityAsync(userId, activity, ct);
    }

    public async Task<ServiceResult<bool>> RecordBadgeEarnedAsync(string userId, string badgeTitle, CancellationToken ct = default)
    {
        var activity = ActivityHelper.CreateBadgeEarnedActivity(userId, badgeTitle);
        return await RecordActivityAsync(userId, activity, ct);
    }

    public async Task<ServiceResult<bool>> RecordStreakMilestoneAsync(string userId, int streakCount, CancellationToken ct = default)
    {
        var activity = ActivityHelper.CreateStreakMilestoneActivity(userId, streakCount);
        return await RecordActivityAsync(userId, activity, ct);
    }

    public async Task<ServiceResult<bool>> RecordLevelUpAsync(string userId, int newLevel, CancellationToken ct = default)
    {
        var activity = ActivityHelper.CreateLevelUpActivity(userId, newLevel);
        return await RecordActivityAsync(userId, activity, ct);
    }

    public async Task<ServiceResult<bool>> RecordQuizCompletedAsync(string userId, string quizTitle, double accuracy, int xpEarned, CancellationToken ct = default)
    {
        var activity = new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.QuizCompleted,
            Title = "Quiz Completed",
            Description = $"Completed '{quizTitle}' with {accuracy:P0} accuracy and earned {xpEarned} XP",
            Icon = "🧠",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["quizTitle"] = quizTitle,
                ["accuracy"] = accuracy,
                ["xpEarned"] = xpEarned
            }
        };

        return await RecordActivityAsync(userId, activity, ct);
    }

    public async Task<ServiceResult<bool>> ClearOldActivitiesAsync(string userId, DateTime olderThan, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<bool>.Failure("User ID is required");

            var activitiesResult = await GetRecentActivitiesAsync(userId, int.MaxValue, ct);
            if (!activitiesResult.IsSuccess || activitiesResult.Data == null)
                return ServiceResult<bool>.Success(true); // No activities to clear

            var oldActivities = activitiesResult.Data
                .Where(a => a.Timestamp < olderThan)
                .ToList();

            var batchOperations = oldActivities
                .Select(a => ($"users/{userId}/activities", a.Id, (object)a, BatchAction.Delete))
                .ToList();

            if (batchOperations.Any())
            {
                var result = await _firestoreRepository.BatchWriteAsync(batchOperations, ct);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Cleared {Count} old activities for user {UserId}", oldActivities.Count, userId);
                }
                return result;
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing old activities for user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Error clearing old activities: {ex.Message}");
        }
    }
}