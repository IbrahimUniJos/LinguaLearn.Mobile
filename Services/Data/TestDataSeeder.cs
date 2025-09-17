using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Services.Activity;
using LinguaLearn.Mobile.Services.User;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.Services.Data;

/// <summary>
/// Service for seeding test data during development
/// </summary>
public class TestDataSeeder
{
    private readonly IUserService _userService;
    private readonly IActivityService _activityService;
    private readonly ILogger<TestDataSeeder> _logger;

    public TestDataSeeder(
        IUserService userService,
        IActivityService activityService,
        ILogger<TestDataSeeder> logger)
    {
        _userService = userService;
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Seeds sample data for a user if they don't have any activities
    /// </summary>
    public async Task SeedSampleDataAsync(string userId)
    {
        try
        {
            // Check if user already has activities
            var existingActivities = await _activityService.GetRecentActivitiesAsync(userId, 1);
            if (existingActivities.IsSuccess && existingActivities.Data?.Any() == true)
            {
                _logger.LogInformation("User {UserId} already has activities, skipping seed", userId);
                return;
            }

            _logger.LogInformation("Seeding sample data for user {UserId}", userId);

            // Create sample activities
            var activities = new[]
            {
                ActivityHelper.CreateLessonCompletedActivity(userId, "Basic Greetings", 50),
                CreateQuizCompletedActivity(userId, "Vocabulary Quiz #1", 85, 30),
                ActivityHelper.CreateStreakMilestoneActivity(userId, 7),
                ActivityHelper.CreateBadgeEarnedActivity(userId, "First Week"),
                ActivityHelper.CreateLevelUpActivity(userId, 2)
            };

            // Add activities with different timestamps
            for (int i = 0; i < activities.Length; i++)
            {
                activities[i].Timestamp = DateTime.UtcNow.AddDays(-i).AddHours(-i);
                await _activityService.RecordActivityAsync(userId, activities[i]);
            }

            // Update user stats
            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile.IsSuccess && userProfile.Data != null)
            {
                var profile = userProfile.Data;
                profile.XP = 250;
                profile.Level = 2;
                profile.StreakCount = 7;
                profile.LastActiveDate = DateTime.UtcNow;
                
                await _userService.UpdateUserProfileAsync(profile);
            }

            // Create sample user stats
            var userStats = new UserStats
            {
                UserId = userId,
                TotalLessonsCompleted = 5,
                TotalQuizzesCompleted = 3,
                TotalXPEarned = 250,
                AverageQuizAccuracy = 0.85,
                TotalStudyTimeMinutes = 120,
                LongestStreak = 7,
                BadgesEarned = 2,
                CurrentWeek = new WeeklyProgress
                {
                    WeekStartDate = DateTime.UtcNow.StartOfWeek(),
                    LessonsCompleted = 3,
                    XPEarned = 150,
                    Goal = 5,
                    DailyActivity = new Dictionary<string, bool>
                    {
                        ["Monday"] = true,
                        ["Tuesday"] = true,
                        ["Wednesday"] = true,
                        ["Thursday"] = false,
                        ["Friday"] = false,
                        ["Saturday"] = false,
                        ["Sunday"] = false
                    }
                },
                LastUpdated = DateTime.UtcNow
            };

            // Save user stats (this would typically be done through the user service)
            // For now, we'll just log that we would save it
            _logger.LogInformation("Sample data seeded successfully for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample data for user {UserId}", userId);
        }
    }

    private static ActivityItem CreateQuizCompletedActivity(string userId, string quizTitle, double accuracy, int xpEarned)
    {
        return new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.QuizCompleted,
            Title = "Quiz Completed",
            Description = $"Completed '{quizTitle}' with {accuracy}% accuracy and earned {xpEarned} XP",
            Icon = "🧠",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["quizTitle"] = quizTitle,
                ["accuracy"] = accuracy / 100.0,
                ["xpEarned"] = xpEarned
            }
        };
    }
}

/// <summary>
/// Extension method to get the start of the week
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}