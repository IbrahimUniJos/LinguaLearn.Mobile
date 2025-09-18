using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

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

    public static string GetDifficultyDisplayName(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => "Beginner",
            "elementary" => "Elementary",
            "intermediate" => "Intermediate",
            "upper-intermediate" => "Upper Intermediate",
            "advanced" => "Advanced",
            _ => difficulty
        };
    }

    public static string GetDifficultyIcon(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => "🟢",
            "elementary" => "🟡",
            "intermediate" => "🟠",
            "upper-intermediate" => "🔴",
            "advanced" => "⚫",
            _ => "⚪"
        };
    }

    public static Color GetDifficultyColor(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => Colors.Green,
            "elementary" => Colors.Yellow,
            "intermediate" => Colors.Orange,
            "upper-intermediate" => Colors.Red,
            "advanced" => Colors.Purple,
            _ => Colors.Gray
        };
    }

    public static int GetDifficultyLevel(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => 1,
            "elementary" => 2,
            "intermediate" => 3,
            "upper-intermediate" => 4,
            "advanced" => 5,
            _ => 0
        };
    }

    public static double CalculateOverallProgress(List<LessonSection> sections, List<string> completedSections)
    {
        if (sections.Count == 0) return 0.0;
        return (double)completedSections.Count / sections.Count;
    }

    public static string FormatEstimatedTime(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} min";
        }
        else
        {
            var hours = minutes / 60;
            var remainingMinutes = minutes % 60;
            return remainingMinutes > 0 ? $"{hours}h {remainingMinutes}m" : $"{hours}h";
        }
    }

    public static bool IsLessonAvailable(Lesson lesson, List<string> completedLessons)
    {
        return lesson.IsActive && ArePrerequisitesMet(lesson.Prerequisites, completedLessons);
    }

    public static int CalculateTotalLessonXP(Lesson lesson)
    {
        return lesson.Sections.Sum(section => CalculateXPForSection(section));
    }

    public static LessonSection? GetNextSection(Lesson lesson, int currentSectionIndex)
    {
        var nextIndex = currentSectionIndex + 1;
        if (nextIndex < lesson.Sections.Count)
        {
            return lesson.Sections.OrderBy(s => s.Order).ElementAtOrDefault(nextIndex);
        }
        return null;
    }

    public static LessonSection? GetPreviousSection(Lesson lesson, int currentSectionIndex)
    {
        var previousIndex = currentSectionIndex - 1;
        if (previousIndex >= 0)
        {
            return lesson.Sections.OrderBy(s => s.Order).ElementAtOrDefault(previousIndex);
        }
        return null;
    }
}