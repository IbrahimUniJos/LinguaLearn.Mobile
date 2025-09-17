using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Progress;

/// <summary>
/// Implementation of progress tracking service
/// </summary>
public class ProgressService : IProgressService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(
        IFirestoreRepository firestoreRepository,
        ILogger<ProgressService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<ProgressRecord>> RecordProgressAsync(ProgressRecord record, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                record.Id = Guid.NewGuid().ToString();
            }

            record.CompletedAt = DateTime.UtcNow;

            var result = await _firestoreRepository.SetDocumentAsync(
                "progress",
                record.Id,
                record,
                ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Progress recorded for user {UserId}, lesson {LessonId}, section {SectionId}",
                    record.UserId, record.LessonId, record.SectionId);
                return ServiceResult<ProgressRecord>.Success(record);
            }

            return ServiceResult<ProgressRecord>.Failure(result.ErrorMessage ?? "Failed to record progress");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording progress for user {UserId}", record.UserId);
            return ServiceResult<ProgressRecord>.Failure($"Error recording progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ProgressRecord>>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default)
    {
        try
        {
            // For now, get all progress records and filter in memory
            // This should be optimized with proper Firestore queries later
            var allRecords = await _firestoreRepository.GetCollectionAsync<ProgressRecord>("progress", ct);
            if (!allRecords.IsSuccess)
            {
                return ServiceResult<List<ProgressRecord>>.Failure(allRecords.ErrorMessage ?? "Failed to get progress records");
            }

            var filteredRecords = allRecords.Data?
                .Where(r => r.UserId == userId && r.LessonId == lessonId)
                .OrderByDescending(r => r.CompletedAt)
                .ToList() ?? new List<ProgressRecord>();

            return ServiceResult<List<ProgressRecord>>.Success(filteredRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user progress for user {UserId}, lesson {LessonId}", userId, lessonId);
            return ServiceResult<List<ProgressRecord>>.Failure($"Error getting user progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserProgress>> CalculateUserProgressAsync(string userId, string lessonId, CancellationToken ct = default)
    {
        try
        {
            // Get existing user progress
            var existingProgressResult = await _firestoreRepository.GetDocumentAsync<UserProgress>(
                "userProgress",
                $"{userId}_{lessonId}",
                ct);

            var userProgress = existingProgressResult.Data ?? new UserProgress
            {
                UserId = userId,
                LessonId = lessonId
            };

            // Get all progress records for this lesson
            var progressRecordsResult = await GetUserProgressAsync(userId, lessonId, ct);
            if (!progressRecordsResult.IsSuccess)
            {
                return ServiceResult<UserProgress>.Failure(progressRecordsResult.ErrorMessage);
            }

            var records = progressRecordsResult.Data ?? new List<ProgressRecord>();

            // Calculate aggregated progress
            userProgress.TotalXPEarned = records.Sum(r => r.XPEarned);
            userProgress.TimeSpentSeconds = records.Sum(r => r.TimeSpentSeconds);
            userProgress.CompletedSections = records.Where(r => r.IsCompleted).Select(r => r.SectionId).Distinct().ToList();
            
            if (records.Any())
            {
                userProgress.Accuracy = records.Average(r => r.Accuracy);
                userProgress.LastAccessedAt = records.Max(r => r.CompletedAt);
            }

            // Update the user progress document
            await _firestoreRepository.SetDocumentAsync(
                "userProgress",
                $"{userId}_{lessonId}",
                userProgress,
                ct);

            return ServiceResult<UserProgress>.Success(userProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating user progress for user {UserId}, lesson {LessonId}", userId, lessonId);
            return ServiceResult<UserProgress>.Failure($"Error calculating user progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<double>> CalculateUserAccuracyAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var allRecords = await _firestoreRepository.GetCollectionAsync<ProgressRecord>("progress", ct);
            if (!allRecords.IsSuccess)
            {
                return ServiceResult<double>.Failure(allRecords.ErrorMessage ?? "Failed to get progress records");
            }

            var userRecords = allRecords.Data?
                .Where(r => r.UserId == userId)
                .Take(100) // Last 100 activities
                .ToList() ?? new List<ProgressRecord>();
            
            if (!userRecords.Any())
            {
                return ServiceResult<double>.Success(0.0);
            }

            var accuracy = userRecords.Average(r => r.Accuracy);
            return ServiceResult<double>.Success(accuracy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating user accuracy for user {UserId}", userId);
            return ServiceResult<double>.Failure($"Error calculating accuracy: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TimeSpan>> CalculateTotalStudyTimeAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var allRecords = await _firestoreRepository.GetCollectionAsync<ProgressRecord>("progress", ct);
            if (!allRecords.IsSuccess)
            {
                return ServiceResult<TimeSpan>.Failure(allRecords.ErrorMessage ?? "Failed to get progress records");
            }

            var userRecords = allRecords.Data?
                .Where(r => r.UserId == userId)
                .ToList() ?? new List<ProgressRecord>();
            
            var totalSeconds = userRecords.Sum(r => r.TimeSpentSeconds);
            return ServiceResult<TimeSpan>.Success(TimeSpan.FromSeconds(totalSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total study time for user {UserId}", userId);
            return ServiceResult<TimeSpan>.Failure($"Error calculating study time: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> CalculateCompletedLessonsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var allProgress = await _firestoreRepository.GetCollectionAsync<UserProgress>("userProgress", ct);
            if (!allProgress.IsSuccess)
            {
                return ServiceResult<int>.Failure(allProgress.ErrorMessage ?? "Failed to get user progress");
            }

            var completedLessons = allProgress.Data?
                .Where(p => p.UserId == userId && p.IsCompleted)
                .Count() ?? 0;

            return ServiceResult<int>.Success(completedLessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating completed lessons for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Error calculating completed lessons: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ReviewItem>>> GetReviewItemsAsync(string userId, int count = 10, CancellationToken ct = default)
    {
        try
        {
            var allReviews = await _firestoreRepository.GetCollectionAsync<ReviewItem>("reviews", ct);
            if (!allReviews.IsSuccess)
            {
                return ServiceResult<List<ReviewItem>>.Failure(allReviews.ErrorMessage ?? "Failed to get review items");
            }

            var reviewItems = allReviews.Data?
                .Where(r => r.UserId == userId && r.NextReviewDate <= DateTime.UtcNow)
                .OrderBy(r => r.NextReviewDate)
                .Take(count)
                .ToList() ?? new List<ReviewItem>();

            return ServiceResult<List<ReviewItem>>.Success(reviewItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review items for user {UserId}", userId);
            return ServiceResult<List<ReviewItem>>.Failure($"Error getting review items: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ScheduleReviewAsync(string userId, string itemId, DateTime reviewDate, CancellationToken ct = default)
    {
        try
        {
            var reviewItem = new ReviewItem
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                ItemId = itemId,
                NextReviewDate = reviewDate,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _firestoreRepository.SetDocumentAsync(
                "reviews",
                reviewItem.Id,
                reviewItem,
                ct);

            return ServiceResult<bool>.Success(result.IsSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling review for user {UserId}, item {ItemId}", userId, itemId);
            return ServiceResult<bool>.Failure($"Error scheduling review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Dictionary<string, object>>> GetProgressStatisticsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var stats = new Dictionary<string, object>();

            // Get accuracy
            var accuracyResult = await CalculateUserAccuracyAsync(userId, ct);
            if (accuracyResult.IsSuccess)
            {
                stats["accuracy"] = accuracyResult.Data;
            }

            // Get total study time
            var studyTimeResult = await CalculateTotalStudyTimeAsync(userId, ct);
            if (studyTimeResult.IsSuccess)
            {
                stats["totalStudyTimeMinutes"] = studyTimeResult.Data.TotalMinutes;
            }

            // Get completed lessons count
            var completedLessonsResult = await CalculateCompletedLessonsAsync(userId, ct);
            if (completedLessonsResult.IsSuccess)
            {
                stats["completedLessons"] = completedLessonsResult.Data;
            }

            // Get recent activity (last 7 days)
            var recentProgressResult = await GetRecentProgressAsync(userId, 7, ct);
            if (recentProgressResult.IsSuccess)
            {
                stats["recentActivities"] = recentProgressResult.Data?.Count ?? 0;
            }

            return ServiceResult<Dictionary<string, object>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress statistics for user {UserId}", userId);
            return ServiceResult<Dictionary<string, object>>.Failure($"Error getting statistics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ProgressRecord>>> GetRecentProgressAsync(string userId, int days = 7, CancellationToken ct = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var allRecords = await _firestoreRepository.GetCollectionAsync<ProgressRecord>("progress", ct);
            if (!allRecords.IsSuccess)
            {
                return ServiceResult<List<ProgressRecord>>.Failure(allRecords.ErrorMessage ?? "Failed to get progress records");
            }

            var recentRecords = allRecords.Data?
                .Where(r => r.UserId == userId && r.CompletedAt >= cutoffDate)
                .OrderByDescending(r => r.CompletedAt)
                .ToList() ?? new List<ProgressRecord>();

            return ServiceResult<List<ProgressRecord>>.Success(recentRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent progress for user {UserId}", userId);
            return ServiceResult<List<ProgressRecord>>.Failure($"Error getting recent progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RecordMultipleProgressAsync(List<ProgressRecord> records, CancellationToken ct = default)
    {
        try
        {
            var tasks = records.Select(record => RecordProgressAsync(record, ct));
            var results = await Task.WhenAll(tasks);
            
            var allSuccessful = results.All(r => r.IsSuccess);
            return ServiceResult<bool>.Success(allSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording multiple progress records");
            return ServiceResult<bool>.Failure($"Error recording multiple progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UpdateProgressBatchAsync(string userId, string lessonId, List<string> completedSections, CancellationToken ct = default)
    {
        try
        {
            // Get current user progress
            var userProgressResult = await _firestoreRepository.GetDocumentAsync<UserProgress>(
                "userProgress",
                $"{userId}_{lessonId}",
                ct);
                
            var userProgress = userProgressResult.Data ?? new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                IsStarted = true,
                StartedAt = DateTime.UtcNow
            };

            // Update completed sections
            foreach (var sectionId in completedSections)
            {
                if (!userProgress.CompletedSections.Contains(sectionId))
                {
                    userProgress.CompletedSections.Add(sectionId);
                }
            }

            userProgress.LastAccessedAt = DateTime.UtcNow;

            // Save updated progress
            var result = await _firestoreRepository.SetDocumentAsync(
                "userProgress",
                $"{userId}_{lessonId}",
                userProgress,
                ct);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress batch for user {UserId}, lesson {LessonId}", userId, lessonId);
            return ServiceResult<bool>.Failure($"Error updating progress batch: {ex.Message}");
        }
    }
}