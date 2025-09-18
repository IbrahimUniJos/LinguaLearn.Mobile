using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Individual section within a lesson
/// </summary>
[FirestoreData]
public class LessonSection
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "vocabulary", "grammar", "pronunciation", "quiz"

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("content")]
    public string Content { get; set; } = string.Empty;

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("order")]
    public int Order { get; set; }
}
