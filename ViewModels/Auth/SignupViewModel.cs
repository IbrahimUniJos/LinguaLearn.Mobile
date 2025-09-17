using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.ViewModels.Messages;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.ViewModels.Auth;

public partial class SignupViewModel : ObservableValidator
{
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<SignupViewModel> _logger;

    // Manual properties using ObservableValidator pattern per docs
    private string email = string.Empty;
    private string password = string.Empty;
    private string displayName = string.Empty;
    private string confirmPassword = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email
    {
        get => email;
        set
        {
            if (SetProperty(ref email, value, validate: true))
            {
                OnPropertyChanged(nameof(CanSignUp));
                SignUpCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password
    {
        get => password;
        set
        {
            if (SetProperty(ref password, value, validate: true))
            {
                // Revalidate ConfirmPassword when Password changes for [Compare] validation
                ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));
                OnPropertyChanged(nameof(CanSignUp));
                SignUpCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [Required(ErrorMessage = "Display name is required")]
    [MinLength(2, ErrorMessage = "Display name must be at least 2 characters")]
    public string DisplayName
    {
        get => displayName;
        set
        {
            if (SetProperty(ref displayName, value, validate: true))
            {
                OnPropertyChanged(nameof(CanSignUp));
                SignUpCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword
    {
        get => confirmPassword;
        set
        {
            if (SetProperty(ref confirmPassword, value, validate: true))
            {
                OnPropertyChanged(nameof(CanSignUp));
                SignUpCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [ObservableProperty]
    private bool agreeToTerms;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public SignupViewModel(IFirebaseAuthService authService, ILogger<SignupViewModel> logger)
    {
        _authService = authService;
        _logger = logger;
        
        // Subscribe to validation errors changed to update CanSignUp
        ErrorsChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Email) || 
                e.PropertyName == nameof(Password) || 
                e.PropertyName == nameof(DisplayName) || 
                e.PropertyName == nameof(ConfirmPassword))
            {
                OnPropertyChanged(nameof(CanSignUp));
                SignUpCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public bool CanSignUp => 
                             AgreeToTerms &&
                             !string.IsNullOrWhiteSpace(Email) &&
                             !string.IsNullOrWhiteSpace(Password) &&
                             !string.IsNullOrWhiteSpace(DisplayName) &&
                             !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                             !GetErrors(nameof(Email)).Any() &&
                             !GetErrors(nameof(Password)).Any() &&
                             !GetErrors(nameof(DisplayName)).Any() &&
                             !GetErrors(nameof(ConfirmPassword)).Any();

    [RelayCommand(CanExecute = nameof(CanSignUp))]
    private async Task SignUpAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // Validate all properties
            ValidateAllProperties();
            // Also revalidate the cross-field compare for safety
            ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));

            if (!CanSignUp)
            {
                _logger.LogWarning("Sign up attempted with invalid form data");
                return;
            }

            _logger.LogInformation("Attempting sign up for user: {Email}", Email);
            var result = await _authService.SignUpWithEmailAsync(Email.Trim(), Password, DisplayName.Trim());

            if (result.IsSuccess)
            {
                _logger.LogInformation("Sign up successful for user: {Email}", Email);
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                await Shell.Current.GoToAsync("//main/home");
            }
            else
            {
                _logger.LogWarning("Sign up failed for user: {Email}. Error: {Error}", Email, result.ErrorMessage);
                ErrorMessage = GetUserFriendlyErrorMessage(result.ErrorMessage);
                await ShowErrorSnackbarAsync(ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign up for user: {Email}", Email);
            ErrorMessage = "An unexpected error occurred. Please try again.";
            await ShowErrorSnackbarAsync(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    partial void OnAgreeToTermsChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSignUp));
        SignUpCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSignUp));
        SignUpCommand.NotifyCanExecuteChanged();
    }

    private string GetUserFriendlyErrorMessage(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "An error occurred. Please try again.";

        return errorMessage.ToLowerInvariant() switch
        {
            var msg when msg.Contains("email-already-in-use") => "An account with this email already exists.",
            var msg when msg.Contains("invalid-email") => "Please enter a valid email address.",
            var msg when msg.Contains("weak-password") => "Password is too weak. Please choose a stronger password.",
            var msg when msg.Contains("user-not-found") => "No account found with this email address.",
            var msg when msg.Contains("wrong-password") => "Incorrect password. Please try again.",
            var msg when msg.Contains("invalid-credential") => "Invalid email or password. Please check your credentials.",
            var msg when msg.Contains("too-many-requests") => "Too many failed attempts. Please try again later.",
            var msg when msg.Contains("network") => "Network error. Please check your internet connection.",
            var msg when msg.Contains("user-disabled") => "This account has been disabled. Please contact support.",
            _ => "An error occurred. Please try again."
        };
    }

    private async Task ShowErrorSnackbarAsync(string message)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(new ShowSnackbarMessage(message, true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show error snackbar");
            await Shell.Current?.DisplayAlert("Error", message, "OK")!;
        }
    }
}