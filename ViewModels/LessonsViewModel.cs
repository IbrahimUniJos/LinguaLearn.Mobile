using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Lessons;
using LinguaLearn.Mobile.Services.User;

namespace LinguaLearn.Mobile.ViewModels;

public partial class LessonsViewModel : ObservableObject
{
    private readonly ILessonService _lessonService;
    private readonly IUserService _userService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedLanguage = "Spanish";

    [ObservableProperty]
    private string _selectedDifficulty = "All";

    public ObservableCollection<Lesson> Lessons { get; } = [];
    public ObservableCollection<Lesson> RecommendedLessons { get; } = [];
    public ObservableCollection<Lesson> ContinueLearningLessons { get; } = [];

    public List<string> Languages { get; } = ["All", "Spanish", "French", "German", "Italian"];
    public List<string> Difficulties { get; } = ["All", "Beginner", "Elementary", "Intermediate", "Advanced"];

    public LessonsViewModel(ILessonService lessonService, IUserService userService)
    {
        _lessonService = lessonService;
        _userService = userService;
    }

    public async Task InitializeAsync()
    {
        await LoadLessonsAsync();
        await LoadRecommendedLessonsAsync();
        await LoadContinueLearningLessonsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadLessonsAsync();
        await LoadRecommendedLessonsAsync();
        await LoadContinueLearningLessonsAsync();
    }

    [RelayCommand]
    private async Task FilterLessonsAsync()
    {
        await LoadLessonsAsync();
    }

    [RelayCommand]
    private async Task StartLessonAsync(Lesson lesson)
    {
        if (lesson == null) return;

        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Please log in to start lessons";
                return;
            }

            // Check prerequisites
            var prerequisitesResult = await _lessonService.ArePrerequisitesMetAsync(userId, lesson.Id);
            if (!prerequisitesResult.IsSuccess || !prerequisitesResult.Data)
            {
                await Shell.Current.DisplayAlert(
                    "Prerequisites Required", 
                    "You need to complete the prerequisite lessons first.", 
                    "OK");
                return;
            }

            // Navigate to lesson player
            await Shell.Current.GoToAsync($"lessonPlayer?lessonId={lesson.Id}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting lesson: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ViewLessonDetailsAsync(Lesson lesson)
    {
        if (lesson == null) return;

        await Shell.Current.GoToAsync($"lessonDetails?lessonId={lesson.Id}");
    }

    private async Task LoadLessonsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            ServiceResult<List<Lesson>> result;

            if (SelectedLanguage != "All" && SelectedDifficulty != "All")
            {
                // Filter by both language and difficulty
                var languageResult = await _lessonService.GetLessonsByLanguageAsync(SelectedLanguage);
                if (languageResult.IsSuccess && languageResult.Data != null)
                {
                    var filteredLessons = languageResult.Data
                        .Where(l => l.Difficulty.Equals(SelectedDifficulty, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    result = ServiceResult<List<Lesson>>.Success(filteredLessons);
                }
                else
                {
                    result = languageResult;
                }
            }
            else if (SelectedLanguage != "All")
            {
                result = await _lessonService.GetLessonsByLanguageAsync(SelectedLanguage);
            }
            else if (SelectedDifficulty != "All")
            {
                result = await _lessonService.GetLessonsByDifficultyAsync(SelectedDifficulty);
            }
            else
            {
                result = await _lessonService.GetLessonsAsync();
            }

            if (result.IsSuccess && result.Data != null)
            {
                Lessons.Clear();
                foreach (var lesson in result.Data)
                {
                    Lessons.Add(lesson);
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to load lessons";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading lessons: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRecommendedLessonsAsync()
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return;

            var result = await _lessonService.GetRecommendedLessonsAsync(userId, 5);
            if (result.IsSuccess && result.Data != null)
            {
                RecommendedLessons.Clear();
                foreach (var lesson in result.Data)
                {
                    RecommendedLessons.Add(lesson);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading recommended lessons: {ex.Message}");
        }
    }

    private async Task LoadContinueLearningLessonsAsync()
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId)) return;

            var result = await _lessonService.GetContinueLearningLessonsAsync(userId, 3);
            if (result.IsSuccess && result.Data != null)
            {
                ContinueLearningLessons.Clear();
                foreach (var lesson in result.Data)
                {
                    ContinueLearningLessons.Add(lesson);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading continue learning lessons: {ex.Message}");
        }
    }

    public string GetLessonProgressText(Lesson lesson)
    {
        // This would be calculated based on user progress
        // For now, return placeholder text
        return "Not Started";
    }

    public Color GetLessonProgressColor(Lesson lesson)
    {
        // This would be based on actual progress
        return Colors.Gray;
    }

    public string GetDifficultyIcon(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => "🟢",
            "elementary" => "🟡",
            "intermediate" => "🟠",
            "advanced" => "🔴",
            _ => "⚪"
        };
    }

    public string GetEstimatedTimeText(int estimatedMinutes)
    {
        if (estimatedMinutes < 60)
        {
            return $"{estimatedMinutes} min";
        }
        else
        {
            var hours = estimatedMinutes / 60;
            var minutes = estimatedMinutes % 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
    }
}