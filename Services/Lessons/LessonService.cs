using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Data;
using LinguaLearn.Mobile.Services.User;
using LinguaLearn.Mobile.Services.Activity;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LinguaLearn.Mobile.Services.Lessons;

public class LessonService : ILessonService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IUserService _userService;
    private readonly IActivityService _activityService;
    private readonly ILogger<LessonService> _logger;

    public LessonService(
        IFirestoreRepository firestoreRepository,
        IUserService userService,
        IActivityService activityService,
        ILogger<LessonService> logger)
    {
        _firestoreRepository = firestoreRepository;
        _userService = userService;
        _activityService = activityService;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Lesson>>> GetLessonsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetCollectionAsync<Lesson>("lessons", ct);
            if (result.IsSuccess && result.Data != null)
            {
                var activeLessons = result.Data
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.Order)
                    .ToList();
                
                return ServiceResult<List<Lesson>>.Success(activeLessons);
            }
            
            return ServiceResult<List<Lesson>>.Failure(result.ErrorMessage ?? "Failed to get lessons");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lessons");
            return ServiceResult<List<Lesson>>.Failure($"Error getting lessons: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Lesson?>> GetLessonAsync(string lessonId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(lessonId))
                return ServiceResult<Lesson?>.Failure("Lesson ID is required");

            var result = await _firestoreRepository.GetDocumentAsync<Lesson>("lessons", lessonId, ct);
            if (result.IsSuccess)
            {
                return ServiceResult<Lesson?>.Success(result.Data);
            }
            
            return ServiceResult<Lesson?>.Failure(result.ErrorMessage ?? "Failed to get lesson");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
            return ServiceResult<Lesson?>.Failure($"Error getting lesson: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Lesson>>> GetLessonsByLanguageAsync(string language, CancellationToken ct = default)
    {
        try
        {
            var allLessonsResult = await GetLessonsAsync(ct);
            if (allLessonsResult.IsSuccess && allLessonsResult.Data != null)
            {
                var filteredLessons = allLessonsResult.Data
                    .Where(l => l.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                return ServiceResult<List<Lesson>>.Success(filteredLessons);
            }
            
            return ServiceResult<List<Lesson>>.Failure(allLessonsResult.ErrorMessage ?? "Failed to get lessons by language");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lessons by language {Language}", language);
            return ServiceResult<List<Lesson>>.Failure($"Error getting lessons by language: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Lesson>>> GetLessonsByDifficultyAsync(string difficulty, CancellationToken ct = default)
    {
        try
        {
            var allLessonsResult = await GetLessonsAsync(ct);
            if (allLessonsResult.IsSuccess && allLessonsResult.Data != null)
            {
                var filteredLessons = allLessonsResult.Data
                    .Where(l => l.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                return ServiceResult<List<Lesson>>.Success(filteredLessons);
            }
            
            return ServiceResult<List<Lesson>>.Failure(allLessonsResult.ErrorMessage ?? "Failed to get lessons by difficulty");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lessons by difficulty {Difficulty}", difficulty);
            return ServiceResult<List<Lesson>>.Failure($"Error getting lessons by difficulty: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> StartLessonAsync(string userId, string lessonId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(lessonId))
                return ServiceResult<bool>.Failure("User ID and Lesson ID are required");

            // Check if prerequisites are met
            var prerequisitesResult = await ArePrerequisitesMetAsync(userId, lessonId, ct);
            if (!prerequisitesResult.IsSuccess || !prerequisitesResult.Data)
            {
                return ServiceResult<bool>.Failure("Prerequisites not met for this lesson");
            }

            // Create or update user progress
            var progress = new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                IsStarted = true,
                IsCompleted = false,
                CurrentSectionIndex = 0,
                StartedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            var result = await _firestoreRepository.SetDocumentAsync(
                $"users/{userId}/progress", 
                lessonId, 
                progress, 
                ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User {UserId} started lesson {LessonId}", userId, lessonId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting lesson {LessonId} for user {UserId}", lessonId, userId);
            return ServiceResult<bool>.Failure($"Error starting lesson: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CompleteLessonAsync(string userId, string lessonId, int xpEarned, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(lessonId))
                return ServiceResult<bool>.Failure("User ID and Lesson ID are required");

            // Use transaction to ensure consistency
            var operations = new List<(string collection, string documentId, object document, BatchAction action)>();

            // Update user progress
            var progressResult = await GetUserProgressAsync(userId, lessonId, ct);
            if (progressResult.IsSuccess && progressResult.Data != null)
            {
                var progress = progressResult.Data;
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.TotalXPEarned = xpEarned;
                progress.LastAccessedAt = DateTime.UtcNow;

                operations.Add(($"users/{userId}/progress", lessonId, progress, BatchAction.Set));
            }

            // Update user profile with XP
            var userResult = await _userService.AddXPAsync(userId, xpEarned, $"Completed lesson {lessonId}", ct);
            if (!userResult.IsSuccess)
            {
                return ServiceResult<bool>.Failure("Failed to update user XP");
            }

            // Record activity
            var lessonResult = await GetLessonAsync(lessonId, ct);
            if (lessonResult.IsSuccess && lessonResult.Data != null)
            {
                await _activityService.RecordLessonCompletedAsync(userId, lessonResult.Data.Title, xpEarned, ct);
            }

            // Execute batch operation
            if (operations.Any())
            {
                var batchResult = await _firestoreRepository.BatchWriteAsync(operations, ct);
                if (batchResult.IsSuccess)
                {
                    _logger.LogInformation("User {UserId} completed lesson {LessonId} and earned {XP} XP", userId, lessonId, xpEarned);
                }
                return batchResult;
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing lesson {LessonId} for user {UserId}", lessonId, userId);
            return ServiceResult<bool>.Failure($"Error completing lesson: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserProgress?>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(lessonId))
                return ServiceResult<UserProgress?>.Failure("User ID and Lesson ID are required");

            var result = await _firestoreRepository.GetDocumentAsync<UserProgress>(
                $"users/{userId}/progress", 
                lessonId, 
                ct);

            return ServiceResult<UserProgress?>.Success(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user progress for lesson {LessonId} and user {UserId}", lessonId, userId);
            return ServiceResult<UserProgress?>.Failure($"Error getting user progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UpdateSectionProgressAsync(string userId, string lessonId, string sectionId, double score, CancellationToken ct = default)
    {
        try
        {
            var progressResult = await GetUserProgressAsync(userId, lessonId, ct);
            if (progressResult.IsSuccess && progressResult.Data != null)
            {
                var progress = progressResult.Data;
                
                // Add section to completed list if not already there
                if (!progress.CompletedSections.Contains(sectionId))
                {
                    progress.CompletedSections.Add(sectionId);
                }

                // Update accuracy (running average)
                var totalSections = progress.CompletedSections.Count;
                progress.Accuracy = ((progress.Accuracy * (totalSections - 1)) + score) / totalSections;
                progress.LastAccessedAt = DateTime.UtcNow;

                var result = await _firestoreRepository.SetDocumentAsync(
                    $"users/{userId}/progress", 
                    lessonId, 
                    progress, 
                    ct);

                return result;
            }

            return ServiceResult<bool>.Failure("User progress not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating section progress for user {UserId}, lesson {LessonId}, section {SectionId}", userId, lessonId, sectionId);
            return ServiceResult<bool>.Failure($"Error updating section progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ArePrerequisitesMetAsync(string userId, string lessonId, CancellationToken ct = default)
    {
        try
        {
            var lessonResult = await GetLessonAsync(lessonId, ct);
            if (!lessonResult.IsSuccess || lessonResult.Data == null)
            {
                return ServiceResult<bool>.Failure("Lesson not found");
            }

            var lesson = lessonResult.Data;
            if (!lesson.Prerequisites.Any())
            {
                return ServiceResult<bool>.Success(true); // No prerequisites
            }

            var completedLessonsResult = await GetCompletedLessonsAsync(userId, ct);
            if (!completedLessonsResult.IsSuccess)
            {
                return ServiceResult<bool>.Failure("Failed to get completed lessons");
            }

            var completedLessons = completedLessonsResult.Data ?? new List<string>();
            var prerequisitesMet = LessonHelper.ArePrerequisitesMet(lesson.Prerequisites, completedLessons);

            return ServiceResult<bool>.Success(prerequisitesMet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking prerequisites for lesson {LessonId} and user {UserId}", lessonId, userId);
            return ServiceResult<bool>.Failure($"Error checking prerequisites: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<string>>> GetCompletedLessonsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var result = await _firestoreRepository.GetCollectionAsync<UserProgress>($"users/{userId}/progress", ct);
            if (result.IsSuccess && result.Data != null)
            {
                var completedLessons = result.Data
                    .Where(p => p.IsCompleted)
                    .Select(p => p.LessonId)
                    .ToList();

                return ServiceResult<List<string>>.Success(completedLessons);
            }

            return ServiceResult<List<string>>.Success(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed lessons for user {UserId}", userId);
            return ServiceResult<List<string>>.Failure($"Error getting completed lessons: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Lesson>>> GetRecommendedLessonsAsync(string userId, int count = 5, CancellationToken ct = default)
    {
        try
        {
            var allLessonsResult = await GetLessonsAsync(ct);
            var completedLessonsResult = await GetCompletedLessonsAsync(userId, ct);

            if (!allLessonsResult.IsSuccess || !completedLessonsResult.IsSuccess)
            {
                return ServiceResult<List<Lesson>>.Failure("Failed to get lesson data");
            }

            var allLessons = allLessonsResult.Data ?? new List<Lesson>();
            var completedLessons = completedLessonsResult.Data ?? new List<string>();

            // Filter out completed lessons and check prerequisites
            var availableLessons = new List<Lesson>();
            foreach (var lesson in allLessons)
            {
                if (completedLessons.Contains(lesson.Id))
                    continue;

                var prerequisitesResult = await ArePrerequisitesMetAsync(userId, lesson.Id, ct);
                if (prerequisitesResult.IsSuccess && prerequisitesResult.Data)
                {
                    availableLessons.Add(lesson);
                }
            }

            // Sort by order and take the requested count
            var recommendedLessons = availableLessons
                .OrderBy(l => l.Order)
                .Take(count)
                .ToList();

            return ServiceResult<List<Lesson>>.Success(recommendedLessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended lessons for user {UserId}", userId);
            return ServiceResult<List<Lesson>>.Failure($"Error getting recommended lessons: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Lesson>>> GetContinueLearningLessonsAsync(string userId, int count = 3, CancellationToken ct = default)
    {
        try
        {
            var progressResult = await _firestoreRepository.GetCollectionAsync<UserProgress>($"users/{userId}/progress", ct);
            if (!progressResult.IsSuccess || progressResult.Data == null)
            {
                return ServiceResult<List<Lesson>>.Success(new List<Lesson>());
            }

            // Get started but not completed lessons
            var inProgressLessons = progressResult.Data
                .Where(p => p.IsStarted && !p.IsCompleted)
                .OrderByDescending(p => p.LastAccessedAt)
                .Take(count)
                .ToList();

            var lessons = new List<Lesson>();
            foreach (var progress in inProgressLessons)
            {
                var lessonResult = await GetLessonAsync(progress.LessonId, ct);
                if (lessonResult.IsSuccess && lessonResult.Data != null)
                {
                    lessons.Add(lessonResult.Data);
                }
            }

            return ServiceResult<List<Lesson>>.Success(lessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting continue learning lessons for user {UserId}", userId);
            return ServiceResult<List<Lesson>>.Failure($"Error getting continue learning lessons: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetCompletedLessonsCountAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var completedLessonsResult = await GetCompletedLessonsAsync(userId, ct);
            if (completedLessonsResult.IsSuccess)
            {
                return ServiceResult<int>.Success(completedLessonsResult.Data?.Count ?? 0);
            }

            return ServiceResult<int>.Failure(completedLessonsResult.ErrorMessage ?? "Failed to get completed lessons count");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed lessons count for user {UserId}", userId);
            return ServiceResult<int>.Failure($"Error getting completed lessons count: {ex.Message}");
        }
    }

    public async Task<ServiceResult<double>> GetAverageAccuracyAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var progressResult = await _firestoreRepository.GetCollectionAsync<UserProgress>($"users/{userId}/progress", ct);
            if (progressResult.IsSuccess && progressResult.Data != null)
            {
                var completedProgress = progressResult.Data.Where(p => p.IsCompleted).ToList();
                if (completedProgress.Any())
                {
                    var averageAccuracy = completedProgress.Average(p => p.Accuracy);
                    return ServiceResult<double>.Success(averageAccuracy);
                }
            }

            return ServiceResult<double>.Success(0.0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average accuracy for user {UserId}", userId);
            return ServiceResult<double>.Failure($"Error getting average accuracy: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TimeSpan>> GetTotalStudyTimeAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var progressResult = await _firestoreRepository.GetCollectionAsync<UserProgress>($"users/{userId}/progress", ct);
            if (progressResult.IsSuccess && progressResult.Data != null)
            {
                var totalSeconds = progressResult.Data.Sum(p => p.TimeSpentSeconds);
                return ServiceResult<TimeSpan>.Success(TimeSpan.FromSeconds(totalSeconds));
            }

            return ServiceResult<TimeSpan>.Success(TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total study time for user {UserId}", userId);
            return ServiceResult<TimeSpan>.Failure($"Error getting total study time: {ex.Message}");
        }
    }

    public async IAsyncEnumerable<Lesson> ListenToLessonsAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        // This would be implemented with Firestore real-time listeners
        // For now, return empty enumerable as placeholder
        yield break;
    }

    public async Task<IDisposable> ListenToUserProgressAsync(string userId, string lessonId, Action<UserProgress?> onProgressUpdate, CancellationToken ct = default)
    {
        // This would be implemented with Firestore real-time listeners
        // For now, return empty disposable as placeholder
        return new EmptyDisposable();
    }

    private class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}