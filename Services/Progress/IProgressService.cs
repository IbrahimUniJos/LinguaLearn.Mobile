using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Services.Progress;

/// <summary>
/// Service for tracking and managing user progress
/// </summary>
public interface IProgressService
{
    // Progress Tracking
    Task<ServiceResult<ProgressRecord>> RecordProgressAsync(ProgressRecord record, CancellationToken ct = default);
    Task<ServiceResult<List<ProgressRecord>>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<UserProgress>> CalculateUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    
    // Analytics
    Task<ServiceResult<double>> CalculateUserAccuracyAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<TimeSpan>> CalculateTotalStudyTimeAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> CalculateCompletedLessonsAsync(string userId, CancellationToken ct = default);
    
    // Review System
    Task<ServiceResult<List<ReviewItem>>> GetReviewItemsAsync(string userId, int count = 10, CancellationToken ct = default);
    Task<ServiceResult<bool>> ScheduleReviewAsync(string userId, string itemId, DateTime reviewDate, CancellationToken ct = default);
    
    // Progress Statistics
    Task<ServiceResult<Dictionary<string, object>>> GetProgressStatisticsAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<List<ProgressRecord>>> GetRecentProgressAsync(string userId, int days = 7, CancellationToken ct = default);
    
    // Batch Operations
    Task<ServiceResult<bool>> RecordMultipleProgressAsync(List<ProgressRecord> records, CancellationToken ct = default);
    Task<ServiceResult<bool>> UpdateProgressBatchAsync(string userId, string lessonId, List<string> completedSections, CancellationToken ct = default);
}

/// <summary>
/// Review item for spaced repetition system
/// </summary>
[FirestoreData]
public class ReviewItem
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [FirestoreProperty("itemType")]
    public string ItemType { get; set; } = string.Empty; // "vocabulary", "grammar", "question"

    [FirestoreProperty("content")]
    public string Content { get; set; } = string.Empty;

    [FirestoreProperty("difficulty")]
    public double Difficulty { get; set; } = 1.0;

    [FirestoreProperty("reviewCount")]
    public int ReviewCount { get; set; }

    [FirestoreProperty("correctCount")]
    public int CorrectCount { get; set; }

    [FirestoreProperty("nextReviewDate")]
    public DateTime NextReviewDate { get; set; }

    [FirestoreProperty("lastReviewedAt")]
    public DateTime? LastReviewedAt { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}