using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LinguaLearn.Mobile.Services.Auth;
using LinguaLearn.Mobile.ViewModels.Messages;
using Microsoft.Extensions.Logging;

namespace LinguaLearn.Mobile.ViewModels.Auth;

public partial class LoginViewModel : ObservableValidator
{
    private readonly IFirebaseAuthService _authService;
    private readonly ILogger<LoginViewModel> _logger;

    // Manual properties using ObservableValidator pattern per docs
    private string email = string.Empty;
    private string password = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email
    {
        get => email;
        set
        {
            if (SetProperty(ref email, value, validate: true))
            {
                OnPropertyChanged(nameof(CanLogin));
                LoginCommand.NotifyCanExecuteChanged();
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
                OnPropertyChanged(nameof(CanLogin));
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    public LoginViewModel(IFirebaseAuthService authService, ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _logger = logger;

        // Subscribe to validation errors changed to update CanLogin
        ErrorsChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Email) || e.PropertyName == nameof(Password))
            {
                OnPropertyChanged(nameof(CanLogin));
                LoginCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public bool CanLogin => 
                            !string.IsNullOrWhiteSpace(Email) &&
                            !string.IsNullOrWhiteSpace(Password) &&
                            !GetErrors(nameof(Email)).Any() &&
                            !GetErrors(nameof(Password)).Any();

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            ValidateAllProperties();
            if (!CanLogin)
            {
                _logger.LogWarning("Login attempted with invalid form data");
                return;
            }

            _logger.LogInformation("Attempting login for user: {Email}", Email);
            var result = await _authService.SignInWithEmailAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Login successful for user: {Email}", Email);
                Password = string.Empty; // clear sensitive data
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Email}. Error: {Error}", Email, result.ErrorMessage);
                ErrorMessage = GetUserFriendlyErrorMessage(result.ErrorMessage);
                await ShowErrorSnackbarAsync(ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for user: {Email}", Email);
            ErrorMessage = "An unexpected error occurred. Please try again.";
            await ShowErrorSnackbarAsync(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await ShowErrorSnackbarAsync("Please enter your email address first");
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.SendPasswordResetEmailAsync(Email.Trim());
            if (result.IsSuccess)
            {
                await Shell.Current?.DisplayAlert("Password Reset", "Password reset email sent. Please check your inbox.", "OK")!;
            }
            else
            {
                await ShowErrorSnackbarAsync(GetUserFriendlyErrorMessage(result.ErrorMessage));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email");
            await ShowErrorSnackbarAsync("Failed to send password reset email. Please try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToSignUpAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanLogin));
        LoginCommand.NotifyCanExecuteChanged();
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