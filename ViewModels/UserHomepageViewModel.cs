using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.Services.Data;
using LinguaLearn.Mobile.Services.User;
using LinguaLearn.Mobile.Services.Activity;

namespace LinguaLearn.Mobile.ViewModels;

public partial class UserHomepageViewModel : ObservableObject
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IUserService _userService;
    private readonly IFirebaseAuthService _authService;
    private readonly IActivityService _activityService;
    private readonly TestDataSeeder _testDataSeeder;

    [ObservableProperty]
    private UserProfile? _userProfile;

    [ObservableProperty]
    private UserStats? _userStats;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _userRank;

    [ObservableProperty]
    private double _rankProgress;

    public ObservableCollection<ActivityItem> RecentActivities { get; } = new();

    public UserHomepageViewModel(
        IFirestoreRepository firestoreRepository,
        IUserService userService,
        IFirebaseAuthService authService,
        IActivityService activityService,
        TestDataSeeder testDataSeeder)
    {
        _firestoreRepository = firestoreRepository;
        _userService = userService;
        _authService = authService;
        _activityService = activityService;
        _testDataSeeder = testDataSeeder;
    }

    public async Task InitializeAsync()
    {
        await LoadUserDataAsync();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadUserDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToLessonsAsync()
    {
        await Shell.Current.GoToAsync("//lessons");
    }

    [RelayCommand]
    private async Task NavigateToLeaderboardAsync()
    {
        await Shell.Current.GoToAsync("//leaderboard");
    }

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await Shell.Current.GoToAsync("//profile");
    }

    [RelayCommand]
    private async Task NavigateToDailyChallengeAsync()
    {
        // TODO: Implement daily challenge navigation
        await Shell.Current.DisplayAlert("Coming Soon", "Daily challenges will be available soon!", "OK");
    }

    [RelayCommand]
    private async Task NavigateToPronunciationAsync()
    {
        // TODO: Implement pronunciation practice navigation
        await Shell.Current.DisplayAlert("Coming Soon", "Pronunciation practice will be available soon!", "OK");
    }

    [RelayCommand]
    private async Task NavigateToVocabularyAsync()
    {
        // TODO: Implement vocabulary review navigation
        await Shell.Current.DisplayAlert("Coming Soon", "Vocabulary review will be available soon!", "OK");
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User not authenticated";
                return;
            }

            // Load user profile
            var profileResult = await _userService.GetUserProfileAsync(userId);
            if (profileResult.IsSuccess)
            {
                UserProfile = profileResult.Data;
            }
            else
            {
                ErrorMessage = "Failed to load user profile";
                return;
            }

            // Load user stats
            var statsResult = await _userService.GetUserStatsAsync(userId);
            if (statsResult.IsSuccess)
            {
                UserStats = statsResult.Data;
            }

            // Load recent activities
            await LoadRecentActivitiesAsync(userId);

            // Load leaderboard position
            await LoadLeaderboardPositionAsync(userId);

            // Seed sample data for development (only if no activities exist)
#if DEBUG
            await _testDataSeeder.SeedSampleDataAsync(userId);
#endif
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecentActivitiesAsync(string userId)
    {
        try
        {
            var activitiesResult = await _activityService.GetRecentActivitiesAsync(userId, 5);
            if (activitiesResult.IsSuccess && activitiesResult.Data != null)
            {
                RecentActivities.Clear();
                foreach (var activity in activitiesResult.Data)
                {
                    RecentActivities.Add(activity);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the entire load
            System.Diagnostics.Debug.WriteLine($"Failed to load recent activities: {ex.Message}");
        }
    }

    private async Task LoadLeaderboardPositionAsync(string userId)
    {
        try
        {
            // For now, simulate leaderboard data
            // TODO: Implement actual leaderboard service
            UserRank = new Random().Next(1, 100);
            RankProgress = new Random().NextDouble();
        }
        catch (Exception ex)
        {
            // Log error but don't fail the entire load
            System.Diagnostics.Debug.WriteLine($"Failed to load leaderboard position: {ex.Message}");
        }
    }

    public double GetWeeklyProgressPercentage()
    {
        if (UserStats?.CurrentWeek == null || UserStats.CurrentWeek.Goal <= 0)
            return 0;

        return Math.Min((double)UserStats.CurrentWeek.LessonsCompleted / UserStats.CurrentWeek.Goal, 1.0);
    }

    public int GetXPForNextLevel()
    {
        if (UserProfile == null) return 0;
        
        // Simple level calculation: Level XP = 50 * level^1.7
        var nextLevelXP = (int)(50 * Math.Pow(UserProfile.Level + 1, 1.7));
        return nextLevelXP - UserProfile.XP;
    }

    public string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        var greeting = hour switch
        {
            < 12 => "Good morning",
            < 17 => "Good afternoon",
            _ => "Good evening"
        };

        return UserProfile != null ? $"{greeting}, {UserProfile.DisplayName}!" : greeting;
    }
}