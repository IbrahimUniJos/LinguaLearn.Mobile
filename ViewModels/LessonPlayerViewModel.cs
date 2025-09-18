using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Services.Lessons;
using LinguaLearn.Mobile.Services.User;
using LinguaLearn.Mobile.Services.Progress;
using LinguaLearn.Mobile.Services.Audio;
using LinguaLearn.Mobile.Services.Activity;

namespace LinguaLearn.Mobile.ViewModels;

public partial class LessonPlayerViewModel : ObservableObject, IQueryAttributable
{
    private readonly ILessonService _lessonService;
    private readonly IUserService _userService;
    private readonly IActivityService _activityService;
    private DateTime _sectionStartTime;

    [ObservableProperty]
    private Lesson? _currentLesson;

    [ObservableProperty]
    private LessonSection? _currentSection;

    [ObservableProperty]
    private int _currentSectionIndex;

    [ObservableProperty]
    private bool _isPlayingAudio;

    [ObservableProperty]
    private string? _userAnswer;

    [ObservableProperty]
    private bool _isAnswerCorrect;

    [ObservableProperty]
    private bool _showFeedback;

    [ObservableProperty]
    private int _xpEarned;

    [ObservableProperty]
    private int _totalXpEarned;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string? _feedbackMessage;

    [ObservableProperty]
    private bool _canNavigateNext;

    [ObservableProperty]
    private bool _canNavigatePrevious;

    public ObservableCollection<LessonSection> Sections { get; } = [];

    private readonly IProgressService _progressService;
    private readonly IAudioService _audioService;

    public LessonPlayerViewModel(ILessonService lessonService,
                                 IUserService userService,
                                 IActivityService activityService,
                                 IProgressService progressService,
                                 IAudioService audioService)
    {
        _lessonService = lessonService;
        _userService = userService;
        _activityService = activityService;
        _progressService = progressService;
        _audioService = audioService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("lessonId"))
        {
            var lessonId = query["lessonId"].ToString();
            if (!string.IsNullOrEmpty(lessonId))
            {
                _ = Task.Run(async () => await InitializeAsync(lessonId));
            }
        }
    }

    public async Task InitializeAsync(string lessonId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await _lessonService.GetLessonAsync(lessonId);
            if (result.IsSuccess && result.Data != null)
            {
                CurrentLesson = result.Data;
                await LoadLessonSectionsAsync();
                
                // Start the lesson if not already started
                var userId = await _userService.GetCurrentUserIdAsync();
                if (!string.IsNullOrEmpty(userId))
                {
                    await _lessonService.StartLessonAsync(userId, lessonId);
                    await LoadUserProgressAsync(userId, lessonId);
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to load lesson";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading lesson: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadLessonSectionsAsync()
    {
        if (CurrentLesson == null) return;

        Sections.Clear();
        foreach (var section in CurrentLesson.Sections.OrderBy(s => s.Order))
        {
            Sections.Add(section);
        }

        if (Sections.Any())
        {
            await NavigateToSectionAsync(0);
        }

        UpdateProgress();
    }

    private async Task LoadUserProgressAsync(string userId, string lessonId)
    {
        try
        {
            var progressResult = await _lessonService.GetUserProgressAsync(userId, lessonId);
            if (progressResult.IsSuccess && progressResult.Data != null)
            {
                var progress = progressResult.Data;
                TotalXpEarned = progress.TotalXPEarned;
                
                // Navigate to the current section if lesson was previously started
                if (progress.CurrentSectionIndex > 0 && progress.CurrentSectionIndex < Sections.Count)
                {
                    await NavigateToSectionAsync(progress.CurrentSectionIndex);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the lesson loading
            System.Diagnostics.Debug.WriteLine($"Failed to load user progress: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToSectionAsync(int sectionIndex)
    {
        if (sectionIndex >= 0 && sectionIndex < Sections.Count)
        {
            CurrentSectionIndex = sectionIndex;
            CurrentSection = Sections[sectionIndex];
            _sectionStartTime = DateTime.UtcNow;
            
            // Reset section state
            UserAnswer = string.Empty;
            IsAnswerCorrect = false;
            ShowFeedback = false;
            FeedbackMessage = null;
            
            UpdateNavigationButtons();
            UpdateProgress();
            
            // Auto-play audio for pronunciation sections
            if (CurrentSection.Type == "pronunciation")
            {
                await PlaySectionAudioAsync();
            }
        }
    }

    [RelayCommand]
    private async Task PlaySectionAudioAsync()
    {
        if (CurrentSection?.Metadata.ContainsKey("audioUrl") == true)
        {
            try
            {
                IsPlayingAudio = true;
                var audioUrl = CurrentSection.Metadata["audioUrl"].ToString();
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    // TODO: Implement audio playback service
                    await Task.Delay(2000); // Simulate audio playback
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error playing audio: {ex.Message}";
            }
            finally
            {
                IsPlayingAudio = false;
            }
        }
    }

    [RelayCommand]
    private async Task SubmitAnswerAsync()
    {
        if (string.IsNullOrWhiteSpace(UserAnswer) || CurrentSection == null)
            return;

        try
        {
            IsLoading = true;
            
            // Validate answer based on section type
            var isCorrect = ValidateAnswer(UserAnswer, CurrentSection);
            IsAnswerCorrect = isCorrect;
            ShowFeedback = true;
            
            if (isCorrect)
            {
                // Award XP for correct answers
                var sectionXP = LessonHelper.CalculateXPForSection(CurrentSection, 1.0);
                XpEarned = sectionXP;
                TotalXpEarned += sectionXP;
                FeedbackMessage = $"Correct! +{sectionXP} XP";
                
                // Record section progress
                await RecordSectionProgressAsync(1.0);
            }
            else
            {
                FeedbackMessage = "Incorrect. Try again!";
                XpEarned = 0;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error submitting answer: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToNextSectionAsync()
    {
        if (CurrentSectionIndex < Sections.Count - 1)
        {
            await NavigateToSectionAsync(CurrentSectionIndex + 1);
        }
        else
        {
            await CompleteLessonAsync();
        }
    }

    [RelayCommand]
    private async Task NavigateToPreviousSectionAsync()
    {
        if (CurrentSectionIndex > 0)
        {
            await NavigateToSectionAsync(CurrentSectionIndex - 1);
        }
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (ShowFeedback && IsAnswerCorrect)
        {
            await NavigateToNextSectionAsync();
        }
        else if (ShowFeedback && !IsAnswerCorrect)
        {
            // Reset for retry
            ShowFeedback = false;
            UserAnswer = string.Empty;
        }
        else
        {
            await SubmitAnswerAsync();
        }
    }

    private async Task RecordSectionProgressAsync(double accuracy)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            if (!string.IsNullOrEmpty(userId) && CurrentLesson != null && CurrentSection != null)
            {
                var timeSpent = (int)(DateTime.UtcNow - _sectionStartTime).TotalSeconds;
                
                var progressRecord = new ProgressRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    LessonId = CurrentLesson.Id,
                    SectionId = CurrentSection.Id,
                    Score = accuracy,
                    Accuracy = accuracy,
                    TimeSpentSeconds = timeSpent,
                    XPEarned = XpEarned,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                // Update section progress
                await _lessonService.UpdateSectionProgressAsync(
                    userId, 
                    CurrentLesson.Id, 
                    CurrentSection.Id, 
                    accuracy);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to record section progress: {ex.Message}");
        }
    }

    private async Task CompleteLessonAsync()
    {
        try
        {
            IsLoading = true;
            var userId = await _userService.GetCurrentUserIdAsync();
            
            if (!string.IsNullOrEmpty(userId) && CurrentLesson != null)
            {
                var result = await _lessonService.CompleteLessonAsync(
                    userId, 
                    CurrentLesson.Id, 
                    TotalXpEarned);
                    
                if (result.IsSuccess)
                {
                    // Navigate to completion screen
                    await Shell.Current.GoToAsync($"//lessonComplete?xp={TotalXpEarned}&lessonId={CurrentLesson.Id}&title={Uri.EscapeDataString(CurrentLesson.Title)}");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Failed to complete lesson";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error completing lesson: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ValidateAnswer(string userAnswer, LessonSection section)
    {
        // Implementation depends on section type and metadata
        if (section.Metadata.ContainsKey("correctAnswer"))
        {
            var correctAnswer = section.Metadata["correctAnswer"].ToString();
            return string.Equals(userAnswer.Trim(), correctAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // For sections without specific answers, consider any non-empty answer as correct
        return !string.IsNullOrWhiteSpace(userAnswer);
    }

    private void UpdateNavigationButtons()
    {
        CanNavigatePrevious = CurrentSectionIndex > 0;
        CanNavigateNext = CurrentSectionIndex < Sections.Count - 1;
    }

    private void UpdateProgress()
    {
        if (Sections.Count > 0)
        {
            ProgressPercentage = (double)(CurrentSectionIndex + 1) / Sections.Count;
        }
        else
        {
            ProgressPercentage = 0;
        }
    }

    public string GetSectionTypeDisplayName(string sectionType)
    {
        return sectionType switch
        {
            "vocabulary" => "Vocabulary",
            "grammar" => "Grammar",
            "pronunciation" => "Pronunciation",
            "quiz" => "Quiz",
            "reading" => "Reading",
            "listening" => "Listening",
            _ => "Lesson"
        };
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

    public bool RequiresInput(string sectionType)
    {
        return sectionType switch
        {
            "vocabulary" => true,
            "grammar" => true,
            "quiz" => true,
            _ => false
        };
    }
}