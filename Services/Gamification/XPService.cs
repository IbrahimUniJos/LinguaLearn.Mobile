namespace LinguaLearn.Mobile.Services.Gamification;

/// <summary>
/// Service for XP calculations and level management as specified in maui-app-specs.md
/// Implements the gamification system with XP calculation and level curve formula
/// </summary>
public interface IXPService
{
    // XP Calculations
    int CalculateBaseXP(string activityType, TimeSpan duration, double accuracy = 1.0);
    int CalculateXPWithMultipliers(int baseXP, int difficultyMultiplier, int streakBonus, double accuracyMultiplier = 1.0);
    int CalculateLessonXP(string lessonDifficulty, TimeSpan completionTime, double accuracy, int streakCount);
    int CalculateQuizXP(int questionsCorrect, int totalQuestions, string difficulty, int streakCount);
    int CalculatePronunciationXP(double pronunciationScore, int streakCount);
    
    // Level Calculations (Quadratic/exponential hybrid: Level XP = 50 * level^1.7)
    int CalculateXPRequiredForLevel(int level);
    int CalculateCurrentLevel(int totalXP);
    int CalculateXPForNextLevel(int totalXP);
    int CalculateXPProgressInCurrentLevel(int totalXP);
    double CalculateProgressPercentageInCurrentLevel(int totalXP);
    
    // Streak Bonuses
    int CalculateStreakBonus(int streakCount);
    double CalculateStreakMultiplier(int streakCount);
    
    // Difficulty Multipliers
    int GetDifficultyMultiplier(string difficulty);
    double GetAccuracyMultiplier(double accuracy);
    
    // Validation
    bool IsValidXPAmount(int xpAmount);
    bool IsValidLevel(int level);
}

/// <summary>
/// Implementation of XP service following the specifications
/// Level Curve: Quadratic/exponential hybrid (Level XP = 50 * level^1.7)
/// XP Calculation: Base XP + difficulty multiplier + streak bonus
/// </summary>
public class XPService : IXPService
{
    // Constants for XP calculations
    private const int BASE_LESSON_XP = 20;
    private const int BASE_QUIZ_XP = 10;
    private const int BASE_PRONUNCIATION_XP = 15;
    private const double LEVEL_EXPONENT = 1.7;
    private const int LEVEL_BASE_XP = 50;
    
    // XP calculation methods
    public int CalculateBaseXP(string activityType, TimeSpan duration, double accuracy = 1.0)
    {
        var baseXP = activityType.ToLowerInvariant() switch
        {
            "lesson" => BASE_LESSON_XP,
            "quiz" => BASE_QUIZ_XP,
            "pronunciation" => BASE_PRONUNCIATION_XP,
            "daily_challenge" => 25,
            "streak_milestone" => 30,
            _ => 5
        };
        
        // Apply accuracy multiplier
        var adjustedXP = (int)(baseXP * accuracy);
        
        // Bonus for longer engagement (capped)
        var durationBonus = Math.Min((int)(duration.TotalMinutes * 2), baseXP / 2);
        
        return adjustedXP + durationBonus;
    }
    
    public int CalculateXPWithMultipliers(int baseXP, int difficultyMultiplier, int streakBonus, double accuracyMultiplier = 1.0)
    {
        var multipliedXP = (int)(baseXP * difficultyMultiplier * accuracyMultiplier);
        return multipliedXP + streakBonus;
    }
    
    public int CalculateLessonXP(string lessonDifficulty, TimeSpan completionTime, double accuracy, int streakCount)
    {
        var baseXP = CalculateBaseXP("lesson", completionTime, accuracy);
        var difficultyMultiplier = GetDifficultyMultiplier(lessonDifficulty);
        var streakBonus = CalculateStreakBonus(streakCount);
        var accuracyMultiplier = GetAccuracyMultiplier(accuracy);
        
        return CalculateXPWithMultipliers(baseXP, difficultyMultiplier, streakBonus, accuracyMultiplier);
    }
    
    public int CalculateQuizXP(int questionsCorrect, int totalQuestions, string difficulty, int streakCount)
    {
        var accuracy = totalQuestions > 0 ? (double)questionsCorrect / totalQuestions : 0.0;
        var baseXP = questionsCorrect * BASE_QUIZ_XP;
        var difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        var streakBonus = CalculateStreakBonus(streakCount);
        var accuracyMultiplier = GetAccuracyMultiplier(accuracy);
        
        return CalculateXPWithMultipliers(baseXP, difficultyMultiplier, streakBonus, accuracyMultiplier);
    }
    
    public int CalculatePronunciationXP(double pronunciationScore, int streakCount)
    {
        var baseXP = (int)(BASE_PRONUNCIATION_XP * (pronunciationScore / 100.0));
        var streakBonus = CalculateStreakBonus(streakCount);
        
        return baseXP + streakBonus;
    }
    
    // Level calculations using quadratic/exponential hybrid formula
    public int CalculateXPRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        return (int)(LEVEL_BASE_XP * Math.Pow(level, LEVEL_EXPONENT));
    }
    
    public int CalculateCurrentLevel(int totalXP)
    {
        if (totalXP <= 0) return 1;
        
        var level = 1;
        while (CalculateXPRequiredForLevel(level + 1) <= totalXP)
        {
            level++;
        }
        
        return level;
    }
    
    public int CalculateXPForNextLevel(int totalXP)
    {
        var currentLevel = CalculateCurrentLevel(totalXP);
        var xpRequiredForNext = CalculateXPRequiredForLevel(currentLevel + 1);
        return xpRequiredForNext - totalXP;
    }
    
    public int CalculateXPProgressInCurrentLevel(int totalXP)
    {
        var currentLevel = CalculateCurrentLevel(totalXP);
        var xpRequiredForCurrentLevel = CalculateXPRequiredForLevel(currentLevel);
        return totalXP - xpRequiredForCurrentLevel;
    }
    
    public double CalculateProgressPercentageInCurrentLevel(int totalXP)
    {
        var currentLevel = CalculateCurrentLevel(totalXP);
        var xpRequiredForCurrentLevel = CalculateXPRequiredForLevel(currentLevel);
        var xpRequiredForNextLevel = CalculateXPRequiredForLevel(currentLevel + 1);
        var xpInCurrentLevel = totalXP - xpRequiredForCurrentLevel;
        var xpNeededForNextLevel = xpRequiredForNextLevel - xpRequiredForCurrentLevel;
        
        return xpNeededForNextLevel > 0 ? (double)xpInCurrentLevel / xpNeededForNextLevel : 0.0;
    }
    
    // Streak calculations
    public int CalculateStreakBonus(int streakCount)
    {
        return streakCount switch
        {
            < 3 => 0,
            < 7 => 5,
            < 14 => 10,
            < 30 => 15,
            < 60 => 20,
            _ => 25
        };
    }
    
    public double CalculateStreakMultiplier(int streakCount)
    {
        return 1.0 + (streakCount * 0.02); // 2% bonus per day, capped at reasonable level
    }
    
    // Difficulty multipliers
    public int GetDifficultyMultiplier(string difficulty)
    {
        return difficulty?.ToLowerInvariant() switch
        {
            "beginner" or "easy" => 1,
            "intermediate" or "medium" => 2,
            "advanced" or "hard" => 3,
            "expert" or "extreme" => 4,
            _ => 1
        };
    }
    
    public double GetAccuracyMultiplier(double accuracy)
    {
        return accuracy switch
        {
            >= 0.95 => 1.5,  // Perfect/near-perfect
            >= 0.85 => 1.25, // Excellent
            >= 0.75 => 1.1,  // Good
            >= 0.65 => 1.0,  // Average
            >= 0.5 => 0.8,   // Below average
            _ => 0.6         // Poor
        };
    }
    
    // Validation methods
    public bool IsValidXPAmount(int xpAmount)
    {
        return xpAmount >= 0 && xpAmount <= 1000; // Reasonable bounds
    }
    
    public bool IsValidLevel(int level)
    {
        return level >= 1 && level <= 100; // Reasonable level cap
    }
}