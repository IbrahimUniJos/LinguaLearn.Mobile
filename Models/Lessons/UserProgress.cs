using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// User progress for a specific lesson
/// </summary>
[FirestoreData]
public class UserProgress
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("isStarted")]
    public bool IsStarted { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("currentSectionIndex")]
    public int CurrentSectionIndex { get; set; }

    [FirestoreProperty("completedSections")]
    public List<string> CompletedSections { get; set; } = new();

    [FirestoreProperty("totalXPEarned")]
    public int TotalXPEarned { get; set; }

    [FirestoreProperty("accuracy")]
    public double Accuracy { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("startedAt")]
    public DateTime? StartedAt { get; set; }

    [FirestoreProperty("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [FirestoreProperty("lastAccessedAt")]
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}
