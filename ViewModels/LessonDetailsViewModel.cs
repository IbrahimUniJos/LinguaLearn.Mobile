using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Lessons;
using LinguaLearn.Mobile.Services.User;

namespace LinguaLearn.Mobile.ViewModels;

public partial class LessonDetailsViewModel : ObservableObject, IQueryAttributable
{
    private readonly ILessonService _lessonService;
    private readonly IUserService _userService;

    [ObservableProperty]
    private Lesson? _lesson;

    [ObservableProperty]
    private UserProgress? _userProgress;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _canStartLesson;

    [ObservableProperty]
    private string _startButtonText = "Start Lesson";

    public ObservableCollection<LessonSection> Sections { get; } = [];
    public ObservableCollection<string> Prerequisites { get; } = [];

    public LessonDetailsViewModel(ILessonService lessonService, IUserService userService)
    {
        _lessonService = lessonService;
        _userService = userService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("lessonId"))
        {
            var lessonId = query["lessonId"].ToString();
            if (!string.IsNullOrEmpty(lessonId))
            {
                _ = Task.Run(async () => await LoadLessonDetailsAsync(lessonId));
            }
        }
    }

    private async Task LoadLessonDetailsAsync(string lessonId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var lessonResult = await _lessonService.GetLessonAsync(lessonId);
            if (lessonResult.IsSuccess && lessonResult.Data != null)
            {
                Lesson = lessonResult.Data;
                await LoadSectionsAsync();
                await LoadPrerequisitesAsync();
                await LoadUserProgressAsync(lessonId);
                await CheckCanStartLessonAsync(lessonId);
            }
            else
            {
                ErrorMessage = lessonResult.ErrorMessage ?? "Failed to load lesson details";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading lesson details: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSectionsAsync()
    {
        if (Lesson == null) return;

        Sections.Clear();
        foreach (var section in Lesson.Sections.OrderBy(s => s.Order))
        {
            Sections.Add(section);
        }
    }

    private async Task LoadPrerequisitesAsync()
    {
        if (Lesson == null) return;

        Prerequisites.Clear();
        foreach (var prerequisiteId in Lesson.Prerequisites)
        {
            var prerequisiteResult = await _lessonService.GetLessonAsync(prerequisiteId);
            if (prerequisiteResult.IsSuccess && prerequisiteResult.Data != null)
            {
                Prerequisites.Add(prerequisiteResult.Data.Title);
            }
        }
    }

    private async Task LoadUserProgressAsync(string lessonId)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (!string.IsNullOrEmpty(userId))
            {
                var progressResult = await _lessonService.GetUserProgressAsync(userId, lessonId);
                if (progressResult.IsSuccess)
                {
                    UserProgress = progressResult.Data;
                    UpdateStartButtonText();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load user progress: {ex.Message}");
        }
    }

    private async Task CheckCanStartLessonAsync(string lessonId)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                CanStartLesson = false;
                StartButtonText = "Login Required";
                return;
            }

            var prerequisitesResult = await _lessonService.ArePrerequisitesMetAsync(userId, lessonId);
            CanStartLesson = prerequisitesResult.IsSuccess && prerequisitesResult.Data;
            
            if (!CanStartLesson)
            {
                StartButtonText = "Prerequisites Required";
            }
        }
        catch (Exception ex)
        {
            CanStartLesson = false;
            StartButtonText = "Error";
            System.Diagnostics.Debug.WriteLine($"Failed to check prerequisites: {ex.Message}");
        }
    }

    private void UpdateStartButtonText()
    {
        if (UserProgress == null)
        {
            StartButtonText = "Start Lesson";
        }
        else if (UserProgress.IsCompleted)
        {
            StartButtonText = "Restart Lesson";
        }
        else if (UserProgress.IsStarted)
        {
            StartButtonText = "Continue Lesson";
        }
        else
        {
            StartButtonText = "Start Lesson";
        }
    }

    [RelayCommand]
    private async Task StartLessonAsync()
    {
        if (Lesson == null || !CanStartLesson) return;

        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Please log in to start lessons";
                return;
            }

            await Shell.Current.GoToAsync($"lessonPlayer?lessonId={Lesson.Id}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting lesson: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public string GetSectionTypeIcon(string sectionType)
    {
        return sectionType switch
        {
            "vocabulary" => "📚",
            "grammar" => "📝",
            "pronunciation" => "🎤",
            "quiz" => "🧠",
            "reading" => "📖",
            "listening" => "👂",
            _ => "📄"
        };
    }

    public string GetDifficultyColor(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "beginner" => "#4CAF50",
            "elementary" => "#FFC107",
            "intermediate" => "#FF9800",
            "advanced" => "#F44336",
            _ => "#9E9E9E"
        };
    }

    public double GetProgressPercentage()
    {
        if (UserProgress == null || Lesson == null || Lesson.Sections.Count == 0)
            return 0;

        return (double)UserProgress.CompletedSections.Count / Lesson.Sections.Count * 100;
    }
}