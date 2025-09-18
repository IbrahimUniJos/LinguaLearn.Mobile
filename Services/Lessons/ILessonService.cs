using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Lessons;

/// <summary>
/// Service for managing lessons and user progress
/// </summary>
public interface ILessonService
{
    // Lesson Operations
    Task<ServiceResult<List<Lesson>>> GetLessonsAsync(CancellationToken ct = default);
    Task<ServiceResult<Lesson?>> GetLessonAsync(string lessonId, CancellationToken ct = default);
    Task<ServiceResult<List<Lesson>>> GetLessonsByLanguageAsync(string language, CancellationToken ct = default);
    Task<ServiceResult<List<Lesson>>> GetLessonsByDifficultyAsync(string difficulty, CancellationToken ct = default);
    
    // Lesson Progress
    Task<ServiceResult<bool>> StartLessonAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<bool>> CompleteLessonAsync(string userId, string lessonId, int xpEarned, CancellationToken ct = default);
    Task<ServiceResult<UserProgress?>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<bool>> UpdateSectionProgressAsync(string userId, string lessonId, string sectionId, double score, CancellationToken ct = default);
    
    // Prerequisites
    Task<ServiceResult<bool>> ArePrerequisitesMetAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<List<string>>> GetCompletedLessonsAsync(string userId, CancellationToken ct = default);
    
    // Recommendations
    Task<ServiceResult<List<Lesson>>> GetRecommendedLessonsAsync(string userId, int count = 5, CancellationToken ct = default);
    Task<ServiceResult<List<Lesson>>> GetContinueLearningLessonsAsync(string userId, int count = 3, CancellationToken ct = default);
    
    // Statistics
    Task<ServiceResult<int>> GetCompletedLessonsCountAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<double>> GetAverageAccuracyAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<TimeSpan>> GetTotalStudyTimeAsync(string userId, CancellationToken ct = default);
    
    // Real-time listeners
    IAsyncEnumerable<Lesson> ListenToLessonsAsync(CancellationToken ct = default);
    Task<IDisposable> ListenToUserProgressAsync(string userId, string lessonId, Action<UserProgress?> onProgressUpdate, CancellationToken ct = default);
    
    // Additional lesson management
    Task<ServiceResult<bool>> ResetLessonProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<List<Lesson>>> SearchLessonsAsync(string searchTerm, CancellationToken ct = default);
    Task<ServiceResult<bool>> BookmarkLessonAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<bool>> UnbookmarkLessonAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<List<Lesson>>> GetBookmarkedLessonsAsync(string userId, CancellationToken ct = default);
}