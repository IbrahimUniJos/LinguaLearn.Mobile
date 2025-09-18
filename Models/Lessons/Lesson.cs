using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Main lesson document stored in Firestore lessons/{lessonId}
/// </summary>
[FirestoreData]
public class Lesson
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("language")]
    public string Language { get; set; } = string.Empty;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [FirestoreProperty("order")]
    public int Order { get; set; }

    [FirestoreProperty("prerequisites")]
    public List<string> Prerequisites { get; set; } = new();

    [FirestoreProperty("sections")]
    public List<LessonSection> Sections { get; set; } = new();

    [FirestoreProperty("estimatedDurationMinutes")]
    public int EstimatedDurationMinutes { get; set; }

    [FirestoreProperty("xpReward")]
    public int XPReward { get; set; }

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
