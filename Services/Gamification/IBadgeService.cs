using System.Text.Json.Serialization;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Gamification;

/// <summary>
/// Service for managing badges as specified in maui-app-specs.md
/// Implements event-driven badge awarding system
/// </summary>
public interface IBadgeService
{
    // Badge awarding
    Task<ServiceResult<bool>> AwardBadgeAsync(string userId, string badgeId, string reason, CancellationToken ct = default);
    Task<ServiceResult<bool>> CheckAndAwardAsync(string userId, string eventType, object eventData, CancellationToken ct = default);
    
    // Badge queries
    Task<ServiceResult<List<UserBadge>>> GetUserBadgesAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> HasBadgeAsync(string userId, string badgeId, CancellationToken ct = default);
    Task<ServiceResult<List<BadgeDefinition>>> GetAllBadgeDefinitionsAsync(CancellationToken ct = default);
    Task<ServiceResult<BadgeDefinition?>> GetBadgeDefinitionAsync(string badgeId, CancellationToken ct = default);
    
    // Badge progress tracking
    Task<ServiceResult<Dictionary<string, int>>> GetBadgeProgressAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> UpdateBadgeProgressAsync(string userId, string badgeId, int progress, CancellationToken ct = default);
    
    // Badge categories
    Task<ServiceResult<List<BadgeDefinition>>> GetBadgesByCategoryAsync(BadgeCategory category, CancellationToken ct = default);
    Task<ServiceResult<List<BadgeDefinition>>> GetAvailableBadgesAsync(string userId, CancellationToken ct = default);
}

/// <summary>
/// Badge definition stored in Firestore badges/definitions/{badgeId}
/// </summary>
public class BadgeDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public BadgeCategory Category { get; set; }
    
    [JsonPropertyName("rarity")]
    public BadgeRarity Rarity { get; set; }
    
    [JsonPropertyName("criteria")]
    public BadgeCriteria Criteria { get; set; } = new();
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
    
    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; } = 0;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Badge criteria for earning conditions
/// </summary>
public class BadgeCriteria
{
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("targetValue")]
    public int TargetValue { get; set; } = 1;
    
    [JsonPropertyName("progressType")]
    public ProgressType ProgressType { get; set; } = ProgressType.Cumulative;
    
    [JsonPropertyName("conditions")]
    public Dictionary<string, object> Conditions { get; set; } = new();
}

/// <summary>
/// Badge categories for organization
/// </summary>
public enum BadgeCategory
{
    Lessons,
    Streaks,
    Quizzes,
    Pronunciation,
    Social,
    Milestones,
    Achievements,
    Special
}

/// <summary>
/// Badge rarity levels
/// </summary>
public enum BadgeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Progress tracking types
/// </summary>
public enum ProgressType
{
    Cumulative,    // Total count (e.g., total lessons completed)
    Consecutive,   // Consecutive count (e.g., streak days)
    Achievement,   // One-time achievement (e.g., perfect score)
    Milestone      // Specific milestone reached (e.g., level 10)
}

/// <summary>
/// Badge events for triggering award checks
/// </summary>
public static class BadgeEvents
{
    public const string LessonCompleted = "lesson_completed";
    public const string QuizCompleted = "quiz_completed";
    public const string PronunciationPracticed = "pronunciation_practiced";
    public const string StreakExtended = "streak_extended";
    public const string LevelUp = "level_up";
    public const string PerfectScore = "perfect_score";
    public const string FirstLesson = "first_lesson";
    public const string WeeklyGoalMet = "weekly_goal_met";
    public const string LongStudySession = "long_study_session";
    public const string EarlyBird = "early_bird";
    public const string NightOwl = "night_owl";
}

/// <summary>
/// Predefined badge definitions for the app
/// </summary>
public static class PredefinedBadges
{
    public static readonly List<BadgeDefinition> All = new()
    {
        // Lesson badges
        new()
        {
            Id = "first_lesson",
            Title = "First Steps",
            Description = "Complete your first lesson",
            Category = BadgeCategory.Lessons,
            Rarity = BadgeRarity.Common,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LessonCompleted,
                TargetValue = 1,
                ProgressType = ProgressType.Cumulative
            }
        },
        new()
        {
            Id = "lesson_10",
            Title = "Getting Started",
            Description = "Complete 10 lessons",
            Category = BadgeCategory.Lessons,
            Rarity = BadgeRarity.Common,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LessonCompleted,
                TargetValue = 10,
                ProgressType = ProgressType.Cumulative
            }
        },
        new()
        {
            Id = "lesson_50",
            Title = "Dedicated Learner",
            Description = "Complete 50 lessons",
            Category = BadgeCategory.Lessons,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LessonCompleted,
                TargetValue = 50,
                ProgressType = ProgressType.Cumulative
            }
        },
        new()
        {
            Id = "lesson_100",
            Title = "Scholar",
            Description = "Complete 100 lessons",
            Category = BadgeCategory.Lessons,
            Rarity = BadgeRarity.Rare,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LessonCompleted,
                TargetValue = 100,
                ProgressType = ProgressType.Cumulative
            }
        },
        
        // Streak badges
        new()
        {
            Id = "streak_3",
            Title = "On a Roll",
            Description = "Maintain a 3-day streak",
            Category = BadgeCategory.Streaks,
            Rarity = BadgeRarity.Common,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.StreakExtended,
                TargetValue = 3,
                ProgressType = ProgressType.Consecutive
            }
        },
        new()
        {
            Id = "streak_7",
            Title = "Week Warrior",
            Description = "Maintain a 7-day streak",
            Category = BadgeCategory.Streaks,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.StreakExtended,
                TargetValue = 7,
                ProgressType = ProgressType.Consecutive
            }
        },
        new()
        {
            Id = "streak_30",
            Title = "Month Master",
            Description = "Maintain a 30-day streak",
            Category = BadgeCategory.Streaks,
            Rarity = BadgeRarity.Epic,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.StreakExtended,
                TargetValue = 30,
                ProgressType = ProgressType.Consecutive
            }
        },
        new()
        {
            Id = "streak_365",
            Title = "Year Legend",
            Description = "Maintain a 365-day streak",
            Category = BadgeCategory.Streaks,
            Rarity = BadgeRarity.Legendary,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.StreakExtended,
                TargetValue = 365,
                ProgressType = ProgressType.Consecutive
            }
        },
        
        // Quiz badges
        new()
        {
            Id = "perfect_quiz",
            Title = "Perfectionist",
            Description = "Get 100% on a quiz",
            Category = BadgeCategory.Quizzes,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.PerfectScore,
                TargetValue = 1,
                ProgressType = ProgressType.Achievement
            }
        },
        new()
        {
            Id = "quiz_master",
            Title = "Quiz Master",
            Description = "Complete 50 quizzes",
            Category = BadgeCategory.Quizzes,
            Rarity = BadgeRarity.Rare,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.QuizCompleted,
                TargetValue = 50,
                ProgressType = ProgressType.Cumulative
            }
        },
        
        // Level badges
        new()
        {
            Id = "level_5",
            Title = "Rising Star",
            Description = "Reach level 5",
            Category = BadgeCategory.Milestones,
            Rarity = BadgeRarity.Common,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LevelUp,
                TargetValue = 5,
                ProgressType = ProgressType.Milestone
            }
        },
        new()
        {
            Id = "level_10",
            Title = "Double Digits",
            Description = "Reach level 10",
            Category = BadgeCategory.Milestones,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LevelUp,
                TargetValue = 10,
                ProgressType = ProgressType.Milestone
            }
        },
        new()
        {
            Id = "level_25",
            Title = "Expert",
            Description = "Reach level 25",
            Category = BadgeCategory.Milestones,
            Rarity = BadgeRarity.Rare,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.LevelUp,
                TargetValue = 25,
                ProgressType = ProgressType.Milestone
            }
        },
        
        // Special time-based badges
        new()
        {
            Id = "early_bird",
            Title = "Early Bird",
            Description = "Complete a lesson before 8 AM",
            Category = BadgeCategory.Special,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.EarlyBird,
                TargetValue = 1,
                ProgressType = ProgressType.Achievement
            }
        },
        new()
        {
            Id = "night_owl",
            Title = "Night Owl",
            Description = "Complete a lesson after 10 PM",
            Category = BadgeCategory.Special,
            Rarity = BadgeRarity.Uncommon,
            Criteria = new BadgeCriteria
            {
                EventType = BadgeEvents.NightOwl,
                TargetValue = 1,
                ProgressType = ProgressType.Achievement
            }
        }
    };
}