using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Quiz document stored in Firestore quizzes/{quizId}
/// </summary>
[FirestoreData]
public class Quiz
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("questions")]
    public List<QuizQuestion> Questions { get; set; } = new();

    [FirestoreProperty("timeLimit")]
    public int TimeLimit { get; set; } // in seconds

    [FirestoreProperty("passingScore")]
    public int PassingScore { get; set; } = 70; // percentage

    [FirestoreProperty("adaptiveSettings")]
    public AdaptiveConfig AdaptiveSettings { get; set; } = new();

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual quiz question
/// </summary>
[FirestoreData]
public class QuizQuestion
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "multiple_choice", "fill_blank", "matching", etc.

    [FirestoreProperty("question")]
    public string Question { get; set; } = string.Empty;

    [FirestoreProperty("options")]
    public List<string> Options { get; set; } = new();

    [FirestoreProperty("correctAnswers")]
    public List<string> CorrectAnswers { get; set; } = new();

    [FirestoreProperty("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [FirestoreProperty("points")]
    public int Points { get; set; } = 1;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = "medium";

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("order")]
    public int Order { get; set; }
}

/// <summary>
/// Adaptive configuration for quizzes
/// </summary>
[FirestoreData]
public class AdaptiveConfig
{
    [FirestoreProperty("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    [FirestoreProperty("difficultyAdjustmentFactor")]
    public double DifficultyAdjustmentFactor { get; set; } = 0.1;

    [FirestoreProperty("minQuestionsPerSession")]
    public int MinQuestionsPerSession { get; set; } = 5;

    [FirestoreProperty("maxQuestionsPerSession")]
    public int MaxQuestionsPerSession { get; set; } = 20;

    [FirestoreProperty("timePerQuestionSeconds")]
    public int TimePerQuestionSeconds { get; set; } = 30;
}

/// <summary>
/// Quiz session for tracking user progress through a quiz
/// </summary>
[FirestoreData]
public class QuizSession
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("quizId")]
    public string QuizId { get; set; } = string.Empty;

    [FirestoreProperty("currentQuestionIndex")]
    public int CurrentQuestionIndex { get; set; }

    [FirestoreProperty("answers")]
    public List<QuizAnswer> Answers { get; set; } = new();

    [FirestoreProperty("score")]
    public int Score { get; set; }

    [FirestoreProperty("totalQuestions")]
    public int TotalQuestions { get; set; }

    [FirestoreProperty("timeRemaining")]
    public int TimeRemaining { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// User's answer to a quiz question
/// </summary>
[FirestoreData]
public class QuizAnswer
{
    [FirestoreProperty("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [FirestoreProperty("userAnswers")]
    public List<string> UserAnswers { get; set; } = new();

    [FirestoreProperty("isCorrect")]
    public bool IsCorrect { get; set; }

    [FirestoreProperty("pointsEarned")]
    public int PointsEarned { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("answeredAt")]
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Final quiz result
/// </summary>
[FirestoreData]
public class QuizResult
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("quizId")]
    public string QuizId { get; set; } = string.Empty;

    [FirestoreProperty("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [FirestoreProperty("score")]
    public int Score { get; set; }

    [FirestoreProperty("totalPoints")]
    public int TotalPoints { get; set; }

    [FirestoreProperty("accuracy")]
    public double Accuracy { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("xpEarned")]
    public int XPEarned { get; set; }

    [FirestoreProperty("isPassed")]
    public bool IsPassed { get; set; }

    [FirestoreProperty("answers")]
    public List<QuizAnswer> Answers { get; set; } = new();

    [FirestoreProperty("completedAt")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Question types enumeration
/// </summary>
public enum QuestionType
{
    MultipleChoice,
    FillBlank,
    Matching,
    Translation,
    Listening,
    Speaking,
    TrueFalse,
    Ordering
}

/// <summary>
/// Helper methods for quiz operations
/// </summary>
public static class QuizHelper
{
    public static bool ValidateAnswer(QuizQuestion question, List<string> userAnswers)
    {
        if (question.CorrectAnswers == null || !question.CorrectAnswers.Any())
            return false;

        return question.Type switch
        {
            "multiple_choice" => ValidateMultipleChoice(question, userAnswers),
            "fill_blank" => ValidateFillBlank(question, userAnswers),
            "true_false" => ValidateTrueFalse(question, userAnswers),
            "matching" => ValidateMatching(question, userAnswers),
            _ => false
        };
    }

    private static bool ValidateMultipleChoice(QuizQuestion question, List<string> userAnswers)
    {
        if (userAnswers.Count != 1) return false;
        return question.CorrectAnswers.Contains(userAnswers[0], StringComparer.OrdinalIgnoreCase);
    }

    private static bool ValidateFillBlank(QuizQuestion question, List<string> userAnswers)
    {
        if (userAnswers.Count != 1) return false;
        var userAnswer = userAnswers[0].Trim();
        return question.CorrectAnswers.Any(correct => 
            string.Equals(correct.Trim(), userAnswer, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ValidateTrueFalse(QuizQuestion question, List<string> userAnswers)
    {
        if (userAnswers.Count != 1) return false;
        return question.CorrectAnswers.Contains(userAnswers[0], StringComparer.OrdinalIgnoreCase);
    }

    private static bool ValidateMatching(QuizQuestion question, List<string> userAnswers)
    {
        // For matching questions, userAnswers should contain pairs
        return userAnswers.SequenceEqual(question.CorrectAnswers);
    }

    public static int CalculateXPForQuiz(QuizResult result)
    {
        var baseXP = result.TotalPoints;
        var accuracyBonus = (int)(baseXP * result.Accuracy * 0.5);
        var speedBonus = result.TimeSpentSeconds < 300 ? 10 : 0; // Bonus for completing under 5 minutes
        
        return baseXP + accuracyBonus + speedBonus;
    }

    public static double CalculateAccuracy(List<QuizAnswer> answers)
    {
        if (!answers.Any()) return 0.0;
        
        var correctAnswers = answers.Count(a => a.IsCorrect);
        return (double)correctAnswers / answers.Count;
    }
}