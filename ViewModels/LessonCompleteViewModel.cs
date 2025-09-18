using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Services.User;

namespace LinguaLearn.Mobile.ViewModels;

public partial class LessonCompleteViewModel : ObservableObject, IQueryAttributable
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private string _lessonTitle = string.Empty;

    [ObservableProperty]
    private int _xpEarned;

    [ObservableProperty]
    private string _lessonId = string.Empty;

    [ObservableProperty]
    private string _congratulationsMessage = "Congratulations!";

    [ObservableProperty]
    private string _xpMessage = string.Empty;

    public LessonCompleteViewModel(IUserService userService)
    {
        _userService = userService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("xp") && int.TryParse(query["xp"].ToString(), out var xp))
        {
            XpEarned = xp;
            XpMessage = $"You earned {xp} XP!";
        }

        if (query.ContainsKey("lessonId"))
        {
            LessonId = query["lessonId"].ToString() ?? string.Empty;
        }

        if (query.ContainsKey("title"))
        {
            LessonTitle = Uri.UnescapeDataString(query["title"].ToString() ?? string.Empty);
            CongratulationsMessage = $"You completed {LessonTitle}!";
        }
    }

    [RelayCommand]
    private async Task ContinueLearningAsync()
    {
        await Shell.Current.GoToAsync("//lessons");
    }

    [RelayCommand]
    private async Task GoToHomepageAsync()
    {
        await Shell.Current.GoToAsync("//homepage");
    }

    [RelayCommand]
    private async Task ShareAchievementAsync()
    {
        try
        {
            var shareText = $"I just completed '{LessonTitle}' and earned {XpEarned} XP on LinguaLearn! 🎉";
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = shareText,
                Title = "Share Achievement"
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sharing achievement: {ex.Message}");
        }
    }

    public string GetXpCelebrationIcon()
    {
        return XpEarned switch
        {
            >= 100 => "🏆",
            >= 50 => "🥇",
            >= 25 => "🥈",
            _ => "🥉"
        };
    }

    public string GetCelebrationMessage()
    {
        return XpEarned switch
        {
            >= 100 => "Outstanding performance!",
            >= 50 => "Excellent work!",
            >= 25 => "Great job!",
            _ => "Well done!"
        };
    }
}