# Firebase Authentication Specification - LinguaLearn MAUI

## Overview
This specification details the implementation of Firebase Authentication using REST API calls through Refit client for the LinguaLearn MAUI application. This approach provides cross-platform compatibility without relying on platform-specific Firebase SDKs.

## Architecture

### Core Components
- **IFirebaseAuthService**: Main authentication service interface
- **FirebaseAuthService**: Concrete implementation using Refit
- **IFirebaseAuthApi**: Refit interface for Firebase Auth REST API
- **AuthModels**: Data transfer objects for authentication
- **UserSession**: Local session management
- **SecureTokenStorage**: Secure token persistence

## Firebase Auth REST API Integration

### Base Configuration
```csharp
public class FirebaseAuthConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string BaseUrl => "https://identitytoolkit.googleapis.com/v1/accounts";
}
```

### Refit API Interface
```csharp
[Headers("Content-Type: application/json")]
public interface IFirebaseAuthApi
{
    [Post("/signUp")]
    Task<SignUpResponse> SignUpAsync([Body] SignUpRequest request, [Query] string key);
    
    [Post("/signInWithPassword")]
    Task<SignInResponse> SignInAsync([Body] SignInRequest request, [Query] string key);
    
    [Post("/token")]
    Task<RefreshTokenResponse> RefreshTokenAsync([Body] RefreshTokenRequest request, [Query] string key);
    
    [Post("/update")]
    Task<UpdateProfileResponse> UpdateProfileAsync([Body] UpdateProfileRequest request, [Query] string key);
    
    [Post("/delete")]
    Task<DeleteAccountResponse> DeleteAccountAsync([Body] DeleteAccountRequest request, [Query] string key);
    
    [Post("/sendOobCode")]
    Task<SendOobCodeResponse> SendPasswordResetAsync([Body] SendOobCodeRequest request, [Query] string key);
    
    [Post("/resetPassword")]
    Task<ResetPasswordResponse> ResetPasswordAsync([Body] ResetPasswordRequest request, [Query] string key);
}
```

## Data Models

### Request Models
```csharp
public class SignUpRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

public class SignInRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

public class RefreshTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "refresh_token";
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }
    
    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

public class DeleteAccountRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
}

public class SendOobCodeRequest
{
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "PASSWORD_RESET";
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [JsonPropertyName("oobCode")]
    public string OobCode { get; set; } = string.Empty;
    
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = string.Empty;
}
```

### Response Models
```csharp
public class SignUpResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
}

public class SignInResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("registered")]
    public bool Registered { get; set; }
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = string.Empty;
}

public class UpdateProfileResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }
    
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = string.Empty;
}

public class DeleteAccountResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
}

public class SendOobCodeResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = string.Empty;
}
```

### Domain Models
```csharp
public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string IdToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken) && DateTime.UtcNow < ExpiresAt;
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public UserSession? Session { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthErrorType ErrorType { get; set; }
}

public enum AuthErrorType
{
    None,
    InvalidCredentials,
    UserNotFound,
    EmailAlreadyExists,
    WeakPassword,
    NetworkError,
    InvalidToken,
    TokenExpired,
    Unknown
}
```

## Service Implementation

### IFirebaseAuthService Interface
```csharp
public interface IFirebaseAuthService
{
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, string? displayName = null, CancellationToken ct = default);
    Task<AuthResult> SignInWithEmailAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(CancellationToken ct = default);
    Task<AuthResult> UpdateProfileAsync(string? displayName = null, string? photoUrl = null, CancellationToken ct = default);
    Task<bool> SendPasswordResetEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(string oobCode, string newPassword, CancellationToken ct = default);
    Task<bool> DeleteAccountAsync(CancellationToken ct = default);
    Task SignOutAsync();
    Task<UserSession?> GetCurrentSessionAsync();
    bool IsAuthenticated { get; }
    event EventHandler<UserSession?> AuthStateChanged;
}
```

### FirebaseAuthService Implementation
```csharp
public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly IFirebaseAuthApi _authApi;
    private readonly FirebaseAuthConfig _config;
    private readonly ISecureStorage _secureStorage;
    private readonly ILogger<FirebaseAuthService> _logger;
    private UserSession? _currentSession;

    public bool IsAuthenticated => _currentSession?.IsAuthenticated ?? false;
    public event EventHandler<UserSession?> AuthStateChanged;

    public FirebaseAuthService(
        IFirebaseAuthApi authApi,
        IOptions<FirebaseAuthConfig> config,
        ISecureStorage secureStorage,
        ILogger<FirebaseAuthService> logger)
    {
        _authApi = authApi;
        _config = config.Value;
        _secureStorage = secureStorage;
        _logger = logger;
    }

    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, string? displayName = null, CancellationToken ct = default)
    {
        try
        {
            var request = new SignUpRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            };

            var response = await _authApi.SignUpAsync(request, _config.ApiKey);
            var session = MapToUserSession(response);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return new AuthResult { IsSuccess = true, Session = session };
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Sign up failed for email: {Email}", email);
            return new AuthResult 
            { 
                IsSuccess = false, 
                ErrorMessage = ParseFirebaseError(ex),
                ErrorType = MapErrorType(ex)
            };
        }
    }

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var request = new SignInRequest
            {
                Email = email,
                Password = password
            };

            var response = await _authApi.SignInAsync(request, _config.ApiKey);
            var session = MapToUserSession(response);
            
            await SaveSessionAsync(session);
            await SetCurrentSessionAsync(session);

            return new AuthResult { IsSuccess = true, Session = session };
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Sign in failed for email: {Email}", email);
            return new AuthResult 
            { 
                IsSuccess = false, 
                ErrorMessage = ParseFirebaseError(ex),
                ErrorType = MapErrorType(ex)
            };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var currentSession = await GetCurrentSessionAsync();
            if (currentSession == null || string.IsNullOrEmpty(currentSession.RefreshToken))
            {
                return new AuthResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "No refresh token available",
                    ErrorType = AuthErrorType.InvalidToken
                };
            }

            var request = new RefreshTokenRequest
            {
                RefreshToken = currentSession.RefreshToken
            };

            var response = await _authApi.RefreshTokenAsync(request, _config.ApiKey);
            
            var updatedSession = new UserSession
            {
                UserId = response.UserId,
                Email = currentSession.Email,
                DisplayName = currentSession.DisplayName,
                IdToken = response.IdToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn))
            };

            await SaveSessionAsync(updatedSession);
            await SetCurrentSessionAsync(updatedSession);

            return new AuthResult { IsSuccess = true, Session = updatedSession };
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return new AuthResult 
            { 
                IsSuccess = false, 
                ErrorMessage = ParseFirebaseError(ex),
                ErrorType = MapErrorType(ex)
            };
        }
    }

    public async Task<AuthResult> UpdateProfileAsync(string? displayName = null, string? photoUrl = null, CancellationToken ct = default)
    {
        try
        {
            var currentSession = await GetCurrentSessionAsync();
            if (currentSession == null)
            {
                return new AuthResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "User not authenticated",
                    ErrorType = AuthErrorType.InvalidToken
                };
            }

            var request = new UpdateProfileRequest
            {
                IdToken = currentSession.IdToken,
                DisplayName = displayName,
                PhotoUrl = photoUrl
            };

            var response = await _authApi.UpdateProfileAsync(request, _config.ApiKey);
            
            var updatedSession = currentSession with 
            { 
                DisplayName = response.DisplayName,
                IdToken = response.IdToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn))
            };

            await SaveSessionAsync(updatedSession);
            await SetCurrentSessionAsync(updatedSession);

            return new AuthResult { IsSuccess = true, Session = updatedSession };
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Profile update failed");
            return new AuthResult 
            { 
                IsSuccess = false, 
                ErrorMessage = ParseFirebaseError(ex),
                ErrorType = MapErrorType(ex)
            };
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var request = new SendOobCodeRequest { Email = email };
            await _authApi.SendPasswordResetAsync(request, _config.ApiKey);
            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Password reset email failed for: {Email}", email);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string oobCode, string newPassword, CancellationToken ct = default)
    {
        try
        {
            var request = new ResetPasswordRequest 
            { 
                OobCode = oobCode, 
                NewPassword = newPassword 
            };
            await _authApi.ResetPasswordAsync(request, _config.ApiKey);
            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Password reset failed");
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(CancellationToken ct = default)
    {
        try
        {
            var currentSession = await GetCurrentSessionAsync();
            if (currentSession == null) return false;

            var request = new DeleteAccountRequest { IdToken = currentSession.IdToken };
            await _authApi.DeleteAccountAsync(request, _config.ApiKey);
            
            await SignOutAsync();
            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Account deletion failed");
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        await ClearSessionAsync();
        await SetCurrentSessionAsync(null);
    }

    public async Task<UserSession?> GetCurrentSessionAsync()
    {
        if (_currentSession != null) return _currentSession;

        try
        {
            var sessionJson = await _secureStorage.GetAsync("firebase_session");
            if (string.IsNullOrEmpty(sessionJson)) return null;

            var session = JsonSerializer.Deserialize<UserSession>(sessionJson);
            
            // Auto-refresh if token is about to expire
            if (session != null && session.ExpiresAt <= DateTime.UtcNow.AddMinutes(5))
            {
                var refreshResult = await RefreshTokenAsync();
                return refreshResult.IsSuccess ? refreshResult.Session : null;
            }

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve current session");
            return null;
        }
    }

    private async Task SaveSessionAsync(UserSession session)
    {
        var sessionJson = JsonSerializer.Serialize(session);
        await _secureStorage.SetAsync("firebase_session", sessionJson);
    }

    private async Task ClearSessionAsync()
    {
        _secureStorage.Remove("firebase_session");
    }

    private async Task SetCurrentSessionAsync(UserSession? session)
    {
        _currentSession = session;
        AuthStateChanged?.Invoke(this, session);
    }

    private UserSession MapToUserSession(SignUpResponse response)
    {
        return new UserSession
        {
            UserId = response.LocalId,
            Email = response.Email,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn))
        };
    }

    private UserSession MapToUserSession(SignInResponse response)
    {
        return new UserSession
        {
            UserId = response.LocalId,
            Email = response.Email,
            DisplayName = response.DisplayName,
            IdToken = response.IdToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn))
        };
    }

    private string ParseFirebaseError(ApiException ex)
    {
        // Parse Firebase error response and return user-friendly message
        // Implementation depends on Firebase error format
        return ex.Content ?? "Authentication failed";
    }

    private AuthErrorType MapErrorType(ApiException ex)
    {
        // Map HTTP status codes and Firebase error codes to AuthErrorType
        return ex.StatusCode switch
        {
            HttpStatusCode.BadRequest => AuthErrorType.InvalidCredentials,
            HttpStatusCode.NotFound => AuthErrorType.UserNotFound,
            HttpStatusCode.Unauthorized => AuthErrorType.InvalidToken,
            _ => AuthErrorType.Unknown
        };
    }
}
```

## Dependency Injection Setup

### MauiProgram.cs Configuration
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: false);
        
        // Firebase Auth Configuration
        builder.Services.Configure<FirebaseAuthConfig>(
            builder.Configuration.GetSection("FirebaseAuth"));

        // HTTP Client for Refit
        builder.Services.AddHttpClient();
        
        // Refit API Client
        builder.Services.AddRefitClient<IFirebaseAuthApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<FirebaseAuthConfig>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Services
        builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);

        // ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();

        return builder.Build();
    }
}
```

### Configuration (appsettings.json)
```json
{
  "FirebaseAuth": {
    "ApiKey": "your-firebase-api-key",
    "ProjectId": "your-project-id"
  }
}
```

## Error Handling Strategy

### Firebase Error Response Format
```csharp
public class FirebaseErrorResponse
{
    [JsonPropertyName("error")]
    public FirebaseError Error { get; set; }
}

public class FirebaseError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("errors")]
    public FirebaseErrorDetail[] Errors { get; set; }
}

public class FirebaseErrorDetail
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}
```

### Error Mapping
```csharp
private AuthErrorType MapFirebaseErrorToAuthErrorType(string errorMessage)
{
    return errorMessage.ToUpperInvariant() switch
    {
        var msg when msg.Contains("EMAIL_NOT_FOUND") => AuthErrorType.UserNotFound,
        var msg when msg.Contains("INVALID_PASSWORD") => AuthErrorType.InvalidCredentials,
        var msg when msg.Contains("EMAIL_EXISTS") => AuthErrorType.EmailAlreadyExists,
        var msg when msg.Contains("WEAK_PASSWORD") => AuthErrorType.WeakPassword,
        var msg when msg.Contains("INVALID_ID_TOKEN") => AuthErrorType.InvalidToken,
        var msg when msg.Contains("TOKEN_EXPIRED") => AuthErrorType.TokenExpired,
        _ => AuthErrorType.Unknown
    };
}
```

## Security Considerations

### Token Management
- Store tokens in SecureStorage only
- Implement automatic token refresh
- Clear tokens on sign out
- Validate token expiration before API calls

### Network Security
- Use HTTPS only
- Implement certificate pinning (optional)
- Add request/response logging for debugging (exclude sensitive data)

### Input Validation
- Validate email format
- Enforce password complexity
- Sanitize user inputs
- Implement rate limiting on client side

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task SignInWithEmailAsync_ValidCredentials_ReturnsSuccess()
{
    // Arrange
    var mockApi = new Mock<IFirebaseAuthApi>();
    var mockStorage = new Mock<ISecureStorage>();
    var mockLogger = new Mock<ILogger<FirebaseAuthService>>();
    
    var expectedResponse = new SignInResponse
    {
        LocalId = "user123",
        Email = "test@example.com",
        IdToken = "token123",
        RefreshToken = "refresh123",
        ExpiresIn = "3600"
    };
    
    mockApi.Setup(x => x.SignInAsync(It.IsAny<SignInRequest>(), It.IsAny<string>()))
           .ReturnsAsync(expectedResponse);
    
    var service = new FirebaseAuthService(mockApi.Object, Options.Create(new FirebaseAuthConfig()), mockStorage.Object, mockLogger.Object);
    
    // Act
    var result = await service.SignInWithEmailAsync("test@example.com", "password123");
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Session);
    Assert.Equal("user123", result.Session.UserId);
}
```

### Integration Tests
- Test against Firebase Auth emulator
- Validate token refresh flow
- Test network failure scenarios
- Verify secure storage integration

## Performance Considerations

### Optimization Strategies
- Cache user session in memory
- Implement background token refresh
- Use connection pooling for HTTP client
- Minimize API calls through smart caching

### Monitoring
- Track authentication success/failure rates
- Monitor token refresh frequency
- Log performance metrics
- Alert on authentication errors

## Future Enhancements

### OAuth Providers
- Google Sign-In
- Apple Sign-In
- Facebook Login
- Microsoft Account

### Advanced Features
- Multi-factor authentication
- Biometric authentication
- Social login integration
- Custom claims support

This specification provides a comprehensive foundation for implementing Firebase Authentication using REST APIs and Refit in the LinguaLearn MAUI application.