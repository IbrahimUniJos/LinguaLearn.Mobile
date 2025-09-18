using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Progress record for individual activities
/// </summary>
[FirestoreData]
public class ProgressRecord
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [FirestoreProperty("score")]
    public double Score { get; set; }

    [FirestoreProperty("accuracy")]
    public double Accuracy { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("xpEarned")]
    public int XPEarned { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("completedAt")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
