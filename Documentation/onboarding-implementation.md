# Onboarding Implementation Document

## Overview
This document outlines the implementation plan for the onboarding flow in the LinguaLearn Mobile application. After a user creates an account, they will be navigated to the onboarding page to set their language preferences before accessing the main application.

## User Flow
1. User creates account via AuthViewModel
2. User is navigated to OnboardingPage
3. User selects native language and target language
4. User completes onboarding
5. User is navigated to main app page

## Implementation Components

### 1. OnboardingPage.xaml (View)
The onboarding page will include:
- Header with app branding
- Language selection controls for native and target languages
- "Get Started" button
- Skip option (optional)

### 2. OnboardingViewModel.cs (ViewModel)
The ViewModel will handle:
- Language selection logic
- Form validation
- API calls to complete onboarding
- Navigation to main app

### 3. Service Integration
- IUserService.CompleteOnboardingAsync() to save language preferences
- IUserService.GetAvailableLanguagesAsync() to populate language options

## Technical Specifications

### View Model Implementation
```csharp
public partial class OnboardingViewModel : ObservableValidator
{
    private readonly IUserService _userService;
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<OnboardingViewModel> _logger;
    
    [ObservableProperty]
    private List<LanguageOption> _availableLanguages = new();
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please select your native language")]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private LanguageOption? _selectedNativeLanguage;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please select a language to learn")]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    private LanguageOption? _selectedTargetLanguage;
    
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public OnboardingViewModel(IUserService userService, IFirebaseAuthService authService, ILogger<OnboardingViewModel> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }
    
    public bool CanCompleteOnboarding => !IsBusy && 
                                         SelectedNativeLanguage != null && 
                                         SelectedTargetLanguage != null &&
                                         SelectedNativeLanguage.Code != SelectedTargetLanguage.Code && // Can't learn your native language
                                         !GetErrors().Any();

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            
            var result = await _userService.GetAvailableLanguagesAsync();
            if (result.IsSuccess)
            {
                AvailableLanguages = result.Data ?? new List<LanguageOption>();
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
    
    [RelayCommand(CanExecute = nameof(CanCompleteOnboarding))]
    private async Task CompleteOnboardingAsync()
    {
        // Implementation details
    }
}
```

### UI Implementation with Horus MaterialDesignControls
The onboarding page will use the following controls:
- MaterialComboBox for language selection
- Button for the primary action
- Label for instructions and errors
- ActivityIndicator for loading states

### Service Implementation
The IUserService implementation will:
1. Update the user's profile with selected languages
2. Set HasCompletedOnboarding flag to true
3. Return success/failure result