using Microsoft.Extensions.Logging;
using Refit;
using LinguaLearn.Mobile.Models.Auth;
using LinguaLearn.Mobile.Models.Common;
using LinguaLearn.Mobile.Services.Storage;

namespace LinguaLearn.Mobile.Services.Auth;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly IFirebaseAuthApi _authApi;
    private readonly ISecureCredentialService _credentialService;
    private readonly ILogger<FirebaseAuthService> _logger;
    private UserSession? _currentSession;

    public bool IsAuthenticated => _currentSession?.IsAuthenticated ?? false;
    public event EventHandler<UserSession?>? AuthStateChanged;

    public FirebaseAuthService(
        IFirebaseAuthApi authApi,
        ISecureCredentialService credentialService,
        ILogger<FirebaseAuthService> logger)
    {
        _authApi = authApi;
        _credentialService = credentialService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserSession>> SignUpWithEmailAsync(
        string email, 
        string password, 
        string? displayName = null, 
        CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                return ServiceResult<UserSession>.Failure("Firebase API key not configured");
            }

            var request = new SignUpRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            };

            var response = await _authApi.SignUpAsync(request, apiKey);
            var session = MapToUserSession(response);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return ServiceResult<UserSession>.Success(session);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Sign up failed for email: {Email}", email);
            return ServiceResult<UserSession>.Failure($"Sign up failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign up");
            return ServiceResult<UserSession>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<UserSession>> SignInWithEmailAsync(
        string email, 
        string password, 
        CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                return ServiceResult<UserSession>.Failure("Firebase API key not configured");
            }

            var request = new SignInRequest
            {
                Email = email,
                Password = password
            };

            var response = await _authApi.SignInAsync(request, apiKey);
            var session = MapToUserSession(response);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return ServiceResult<UserSession>.Success(session);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Sign in failed for email: {Email}", email);
            return ServiceResult<UserSession>.Failure($"Sign in failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign in");
            return ServiceResult<UserSession>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<UserSession>> RefreshTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            var refreshToken = await _credentialService.GetRefreshTokenAsync();
            
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(refreshToken))
            {
                return ServiceResult<UserSession>.Failure("Missing credentials for token refresh");
            }

            var request = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            var response = await _authApi.RefreshTokenAsync(request, apiKey);
            var session = MapToUserSession(response, _currentSession);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return ServiceResult<UserSession>.Success(session);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            await SignOutAsync(); // Clear invalid tokens
            return ServiceResult<UserSession>.Failure($"Token refresh failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return ServiceResult<UserSession>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<UserSession>> UpdateProfileAsync(
        string? displayName = null, 
        string? photoUrl = null, 
        CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            var idToken = await _credentialService.GetIdTokenAsync();
            
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(idToken))
            {
                return ServiceResult<UserSession>.Failure("User not authenticated");
            }

            var request = new UpdateProfileRequest
            {
                IdToken = idToken,
                DisplayName = displayName,
                PhotoUrl = photoUrl
            };

            var response = await _authApi.UpdateProfileAsync(request, apiKey);
            var session = MapToUserSession(response, _currentSession);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return ServiceResult<UserSession>.Success(session);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Profile update failed");
            return ServiceResult<UserSession>.Failure($"Profile update failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during profile update");
            return ServiceResult<UserSession>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<bool>> SendPasswordResetEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                return ServiceResult<bool>.Failure("Firebase API key not configured");
            }

            var request = new SendOobCodeRequest
            {
                Email = email
            };

            await _authApi.SendPasswordResetAsync(request, apiKey);
            return ServiceResult<bool>.Success(true);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Password reset email failed for: {Email}", email);
            return ServiceResult<bool>.Failure($"Password reset failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAccountAsync(CancellationToken ct = default)
    {
        try
        {
            var apiKey = await _credentialService.GetFirebaseApiKeyAsync();
            var idToken = await _credentialService.GetIdTokenAsync();
            
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(idToken))
            {
                return ServiceResult<bool>.Failure("User not authenticated");
            }

            var request = new DeleteAccountRequest
            {
                IdToken = idToken
            };

            await _authApi.DeleteAccountAsync(request, apiKey);
            await SignOutAsync();
            
            return ServiceResult<bool>.Success(true);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Account deletion failed");
            return ServiceResult<bool>.Failure($"Account deletion failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during account deletion");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    public Task SignOutAsync()
    {
        return Task.Run(async () =>
        {
            await _credentialService.ClearAllTokensAsync();
            _currentSession = null;
            AuthStateChanged?.Invoke(this, null);
        });
    }

    public async Task<UserSession?> GetCurrentSessionAsync()
    {
        if (_currentSession != null && _currentSession.IsAuthenticated)
        {
            return _currentSession;
        }

        // Try to restore from secure storage
        var hasValidCredentials = await _credentialService.HasValidCredentialsAsync();
        if (hasValidCredentials)
        {
            var idToken = await _credentialService.GetIdTokenAsync();
            var refreshToken = await _credentialService.GetRefreshTokenAsync();
            
            if (!string.IsNullOrEmpty(idToken) && !string.IsNullOrEmpty(refreshToken))
            {
                // TODO: Extract user info from JWT token or call getUserData API
                _currentSession = new UserSession
                {
                    IdToken = idToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1) // Approximate
                };
                
                return _currentSession;
            }
        }

        return null;
    }

    private UserSession MapToUserSession(SignUpResponse response)
    {
        var expiresIn = int.TryParse(response.ExpiresIn, out var expiry) ? expiry : 3600;
        
        return new UserSession
        {
            UserId = response.LocalId,
            Email = response.Email,
            DisplayName = response.DisplayName,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiry)
        };
    }

    private UserSession MapToUserSession(SignInResponse response)
    {
        var expiresIn = int.TryParse(response.ExpiresIn, out var expiry) ? expiry : 3600;
        
        return new UserSession
        {
            UserId = response.LocalId,
            Email = response.Email,
            DisplayName = response.DisplayName,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiry)
        };
    }

    private UserSession MapToUserSession(RefreshTokenResponse response, UserSession? currentSession)
    {
        var expiresIn = int.TryParse(response.ExpiresIn, out var expiry) ? expiry : 3600;
        
        return new UserSession
        {
            UserId = response.UserId,
            Email = currentSession?.Email ?? string.Empty,
            DisplayName = currentSession?.DisplayName,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiry)
        };
    }

    private UserSession MapToUserSession(UpdateProfileResponse response, UserSession? currentSession)
    {
        var expiresIn = int.TryParse(response.ExpiresIn, out var expiry) ? expiry : 3600;
        
        return new UserSession
        {
            UserId = response.LocalId,
            Email = response.Email,
            DisplayName = response.DisplayName,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiry)
        };
    }

    private async Task SaveSessionAsync(UserSession session)
    {
        await _credentialService.SetIdTokenAsync(session.IdToken);
        await _credentialService.SetRefreshTokenAsync(session.RefreshToken);
    }

    private async Task SetCurrentSessionAsync(UserSession session)
    {
        _currentSession = session;
        AuthStateChanged?.Invoke(this, session);
    }

    private string ParseFirebaseError(ApiException ex)
    {
        // Parse Firebase error response and return user-friendly message
        var content = ex.Content ?? string.Empty;
        
        if (content.Contains("EMAIL_EXISTS"))
            return "An account with this email already exists";
        if (content.Contains("INVALID_EMAIL"))
            return "Invalid email address";
        if (content.Contains("WEAK_PASSWORD"))
            return "Password is too weak";
        if (content.Contains("EMAIL_NOT_FOUND"))
            return "No account found with this email";
        if (content.Contains("INVALID_PASSWORD"))
            return "Invalid password";
        if (content.Contains("USER_DISABLED"))
            return "This account has been disabled";
        if (content.Contains("TOO_MANY_ATTEMPTS_TRY_LATER"))
            return "Too many failed attempts. Please try again later";
            
        return ex.Message;
    }
}