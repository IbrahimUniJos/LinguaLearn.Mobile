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

/// <summary>
/// Enums for lesson types
/// </summary>
public enum LessonType
{
    Vocabulary,
    Grammar,
    Pronunciation,
    Quiz,
    Reading,
    Listening
}

/// <summary>
/// Helper methods for lesson management
/// </summary>
public static class LessonHelper
{
    public static int CalculateXPForSection(LessonSection section, double accuracy = 1.0)
    {
        var baseXP = section.Type switch
        {
            "vocabulary" => 10,
            "grammar" => 15,
            "pronunciation" => 20,
            "quiz" => 25,
            "reading" => 12,
            "listening" => 18,
            _ => 5
        };

        // Apply accuracy multiplier
        return (int)(baseXP * accuracy);
    }

    public static TimeSpan EstimateCompletionTime(Lesson lesson)
    {
        var baseMinutes = lesson.Sections.Count * 2; // 2 minutes per section base
        var difficultyMultiplier = lesson.Difficulty switch
        {
            "beginner" => 1.0,
            "elementary" => 1.2,
            "intermediate" => 1.5,
            "upper-intermediate" => 1.8,
            "advanced" => 2.0,
            _ => 1.0
        };

        return TimeSpan.FromMinutes(baseMinutes * difficultyMultiplier);
    }

    public static bool ArePrerequisitesMet(List<string> prerequisites, List<string> completedLessons)
    {
        return prerequisites.All(prereq => completedLessons.Contains(prereq));
    }
}