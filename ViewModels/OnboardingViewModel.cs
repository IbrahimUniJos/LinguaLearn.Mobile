using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinguaLearn.Mobile.Models;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.Services.User;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.ViewModels;

/// <summary>
/// Comprehensive onboarding view model that collects user information,
/// language preferences, and app settings for optimal user experience
/// </summary>
public partial class OnboardingViewModel : ObservableValidator
{
    private readonly IUserService _userService;
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<OnboardingViewModel> _logger;

    #region Observable Properties

    [ObservableProperty]
    private List<LanguageOption> _availableLanguages = new();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please select your native language")]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private LanguageOption? _selectedNativeLanguage;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please select a language to learn")]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private LanguageOption? _selectedTargetLanguage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private DifficultyLevel _selectedDifficultyLevel = DifficultyLevel.Adaptive;

    [ObservableProperty]
    private PronunciationSensitivity _selectedPronunciationSensitivity = PronunciationSensitivity.Medium;

    [ObservableProperty]
    private Models.AppTheme _selectedTheme = Models.AppTheme.System;

    [ObservableProperty]
    private bool _soundEnabled = true;

    [ObservableProperty]
    private bool _vibrationEnabled = true;

    [ObservableProperty]
    private bool _dailyReminderEnabled = true;

    [ObservableProperty]
    private TimeSpan _dailyReminderTime = new(19, 0, 0); // 7 PM default

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private int _weeklyGoal = 5; // 5 lessons per week default

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private int _currentStep = 1;

    [ObservableProperty]
    private int _totalSteps = 4;

    #endregion

    #region Step Properties

    public bool IsStep1 => CurrentStep == 1; // Language Selection
    public bool IsStep2 => CurrentStep == 2; // Difficulty & Learning Preferences  
    public bool IsStep3 => CurrentStep == 3; // App Preferences & Notifications
    public bool IsStep4 => CurrentStep == 4; // Theme & Final Settings

    public bool CanGoNext => CurrentStep < TotalSteps && IsCurrentStepValid;
    public bool CanGoPrevious => CurrentStep > 1;
    public bool CanComplete => CurrentStep == TotalSteps && IsCurrentStepValid;

    private bool IsCurrentStepValid => CurrentStep switch
    {
        1 => SelectedNativeLanguage != null && SelectedTargetLanguage != null && 
             SelectedNativeLanguage.Code != SelectedTargetLanguage.Code,
        2 => true, // Difficulty step is always valid (has defaults)
        3 => WeeklyGoal > 0 && WeeklyGoal <= 21, // Reasonable weekly goal range
        4 => true, // Theme step is always valid (has defaults)
        _ => false
    };

    #endregion

    #region Computed Properties

    public List<DifficultyLevel> AvailableDifficultyLevels => 
        Enum.GetValues<DifficultyLevel>().ToList();

    public List<PronunciationSensitivity> AvailablePronunciationSensitivities => 
        Enum.GetValues<PronunciationSensitivity>().ToList();

    public List<Models.AppTheme> AvailableThemes => 
        Enum.GetValues<Models.AppTheme>().ToList();

    public string StepTitle => CurrentStep switch
    {
        1 => "Choose Your Languages",
        2 => "Learning Preferences", 
        3 => "Notifications & Goals",
        4 => "Final Setup",
        _ => "Setup"
    };

    public string StepDescription => CurrentStep switch
    {
        1 => "Let's start by selecting your native language and the language you want to learn.",
        2 => "Help us customize your learning experience based on your skill level and preferences.",
        3 => "Set up notifications and learning goals to stay motivated and on track.",
        4 => "Choose your preferred theme and complete your profile setup.",
        _ => ""
    };

    public string ProgressText => $"Step {CurrentStep} of {TotalSteps}";
    public double ProgressValue => (double)CurrentStep / TotalSteps;

    #endregion

    public OnboardingViewModel(
        IUserService userService, 
        IFirebaseAuthService authService, 
        ILogger<OnboardingViewModel> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    #region Public Methods

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            
            _logger.LogInformation("Initializing onboarding");
            
            var result = await _userService.GetAvailableLanguagesAsync();
            if (result.IsSuccess)
            {
                AvailableLanguages = result.Data ?? new List<LanguageOption>();
                _logger.LogInformation("Loaded {Count} available languages", AvailableLanguages.Count);
            }
            else
            {
                ErrorMessage = "Failed to load language options";
                _logger.LogError("Failed to load language options: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load language options";
            _logger.LogError(ex, "Failed to load language options");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Navigation Commands

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextStep()
    {
        if (CurrentStep < TotalSteps)
        {
            CurrentStep++;
            UpdateStepProperties();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            UpdateStepProperties();
        }
    }

    private void UpdateStepProperties()
    {
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(IsStep4));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanComplete));
        OnPropertyChanged(nameof(StepTitle));
        OnPropertyChanged(nameof(StepDescription));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressValue));
    }

    #endregion

    #region Completion Commands

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteOnboardingAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            
            // Validate all properties before proceeding
            ValidateAllProperties();
            if (!IsCurrentStepValid)
            {
                _logger.LogWarning("Onboarding attempted with invalid form data");
                return;
            }

            _logger.LogInformation("Completing comprehensive onboarding");
            
            // Get current user ID from auth service
            var userId = await GetUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "Unable to identify user. Please log in again.";
                return;
            }

            // Create comprehensive user profile with all preferences
            var session = await _authService.GetCurrentSessionAsync();
            var userProfile = new UserProfile
            {
                Id = userId,
                Email = session?.Email ?? "",
                DisplayName = session?.DisplayName ?? "User",
                NativeLanguage = SelectedNativeLanguage!.Code,
                TargetLanguage = SelectedTargetLanguage!.Code,
                HasCompletedOnboarding = true,
                XP = 0,
                Level = 1,
                StreakCount = 0,
                StreakFreezeTokens = 3, // Give new users 3 freeze tokens
                Preferences = new UserPreferences
                {
                    SoundEnabled = SoundEnabled,
                    VibrationEnabled = VibrationEnabled,
                    DailyReminderEnabled = DailyReminderEnabled,
                    DailyReminderTime = DailyReminderTime,
                    WeeklyGoal = WeeklyGoal,
                    DifficultyPreference = SelectedDifficultyLevel,
                    PronunciationSensitivity = SelectedPronunciationSensitivity,
                    Theme = SelectedTheme
                },
                Badges = new List<UserBadge>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1
            };

            // Create user profile in Firestore
            var createResult = await _userService.CreateUserProfileAsync(userProfile);
            if (!createResult.IsSuccess)
            {
                ErrorMessage = GetUserFriendlyErrorMessage(createResult.ErrorMessage);
                _logger.LogError("Failed to create user profile: {Error}", createResult.ErrorMessage);
                return;
            }

            // Award onboarding completion badge
            await _userService.AwardBadgeAsync(userId, "onboarding_complete");

            _logger.LogInformation("Comprehensive onboarding completed successfully for user: {UserId}", userId);
            
            // Navigate to main app
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during onboarding");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SkipOnboardingAsync()
    {
        try
        {
            _logger.LogInformation("User chose to skip onboarding");
            
            // Create minimal user profile for users who skip
            var userId = await GetUserIdAsync();
            if (!string.IsNullOrEmpty(userId))
            {
                var session = await _authService.GetCurrentSessionAsync();
                var minimalProfile = new UserProfile
                {
                    Id = userId,
                    Email = session?.Email ?? "",
                    DisplayName = session?.DisplayName ?? "User",
                    HasCompletedOnboarding = false, // Mark as not completed
                    Preferences = new UserPreferences(), // Use all defaults
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1
                };

                await _userService.CreateUserProfileAsync(minimalProfile);
            }

            // Navigate to main app
            await Shell.Current.GoToAsync("//main/home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping onboarding");
            ErrorMessage = "Failed to skip onboarding. Please try again.";
        }
    }

    #endregion

    #region Helper Methods

    private async Task<string?> GetUserIdAsync()
    {
        try
        {
            var session = await _authService.GetCurrentSessionAsync();
            return session?.UserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user session");
            return null;
        }
    }

    private string GetUserFriendlyErrorMessage(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "An error occurred. Please try again.";

        return errorMessage.ToLowerInvariant() switch
        {
            var msg when msg.Contains("network") => "Network error. Please check your internet connection.",
            var msg when msg.Contains("permission") => "Permission denied. Please try logging in again.",
            var msg when msg.Contains("not found") => "User information not found. Please try again.",
            _ => "An error occurred. Please try again."
        };
    }

    #endregion

    #region Property Change Handlers

    partial void OnSelectedNativeLanguageChanged(LanguageOption? value)
    {
        ValidateProperty(value, nameof(SelectedNativeLanguage));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanComplete));
    }

    partial void OnSelectedTargetLanguageChanged(LanguageOption? value)
    {
        ValidateProperty(value, nameof(SelectedTargetLanguage));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanComplete));
    }

    partial void OnCurrentStepChanged(int value)
    {
        UpdateStepProperties();
    }

    #endregion
}