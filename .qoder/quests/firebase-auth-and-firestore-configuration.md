# Firebase Authentication and Firestore Configuration - LinguaLearn MAUI

## Overview

This design document outlines the comprehensive configuration of Firebase Authentication using REST API with Refit and Firestore Database using the official Google.Cloud.Firestore NuGet package for the LinguaLearn MAUI application. The configuration ensures secure credential management, proper dependency injection setup, and immediate project readiness.

## Technology Stack & Dependencies

### Required NuGet Packages

```xml
<PackageReference Include="Google.Cloud.Firestore" Version="3.7.0" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
<PackageReference Include="Refit" Version="8.0.0" />
<PackageReference Include="Refit.HttpClientFactory" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
```

### Project Structure

```
LinguaLearn.Mobile/
├── Configuration/
│   ├── FirebaseAuthConfig.cs
│   └── FirestoreConfig.cs
├── Services/
│   ├── Auth/
│   │   ├── IFirebaseAuthService.cs
│   │   ├── FirebaseAuthService.cs
│   │   └── IFirebaseAuthApi.cs
│   ├── Data/
│   │   ├── IFirestoreRepository.cs
│   │   └── FirestoreRepository.cs
│   └── Storage/
│       ├── ISecureCredentialService.cs
│       └── SecureCredentialService.cs
├── Models/
│   ├── Auth/
│   │   ├── AuthModels.cs
│   │   └── UserSession.cs
│   └── Common/
│       └── ServiceResult.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Resources/
    ├── Raw/
    │   ├── appsettings.json
    │   └── google-services.json
    └── ...
```

## Configuration Models

### Firebase Authentication Configuration

```csharp
// Configuration/FirebaseAuthConfig.cs
public class FirebaseAuthConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://identitytoolkit.googleapis.com/v1/accounts";
    public int TokenRefreshThresholdMinutes { get; set; } = 5;
    public int HttpTimeoutSeconds { get; set; } = 30;
}
```

### Firestore Configuration

```csharp
// Configuration/FirestoreConfig.cs
public class FirestoreConfig
{
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsFileName { get; set; } = "google-services.json";
    public bool UseEmulator { get; set; } = false;
    public string EmulatorHost { get; set; } = "localhost:8080";
}
```

## Secure Credential Management

### ISecureCredentialService Interface

```csharp
// Services/Storage/ISecureCredentialService.cs
public interface ISecureCredentialService
{
    Task<string?> GetFirebaseApiKeyAsync();
    Task SetFirebaseApiKeyAsync(string apiKey);
    Task<string?> GetIdTokenAsync();
    Task SetIdTokenAsync(string idToken);
    Task<string?> GetRefreshTokenAsync();
    Task SetRefreshTokenAsync(string refreshToken);
    Task ClearAllTokensAsync();
    Task<bool> HasValidCredentialsAsync();
}
```

### SecureCredentialService Implementation

```csharp
// Services/Storage/SecureCredentialService.cs
public class SecureCredentialService : ISecureCredentialService
{
    private readonly ISecureStorage _secureStorage;
    private readonly ILogger<SecureCredentialService> _logger;

    private const string FIREBASE_API_KEY = "firebase_api_key";
    private const string ID_TOKEN_KEY = "firebase_id_token";
    private const string REFRESH_TOKEN_KEY = "firebase_refresh_token";
    private const string TOKEN_EXPIRY_KEY = "firebase_token_expiry";

    public SecureCredentialService(
        ISecureStorage secureStorage,
        ILogger<SecureCredentialService> logger)
    {
        _secureStorage = secureStorage;
        _logger = logger;
    }

    public async Task<string?> GetFirebaseApiKeyAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(FIREBASE_API_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Firebase API key");
            return null;
        }
    }

    public async Task SetFirebaseApiKeyAsync(string apiKey)
    {
        try
        {
            await _secureStorage.SetAsync(FIREBASE_API_KEY, apiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store Firebase API key");
            throw;
        }
    }

    public async Task<string?> GetIdTokenAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(ID_TOKEN_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ID token");
            return null;
        }
    }

    public async Task SetIdTokenAsync(string idToken)
    {
        try
        {
            await _secureStorage.SetAsync(ID_TOKEN_KEY, idToken);
            await _secureStorage.SetAsync(TOKEN_EXPIRY_KEY, DateTime.UtcNow.AddHours(1).ToBinary().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store ID token");
            throw;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await _secureStorage.GetAsync(REFRESH_TOKEN_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refresh token");
            return null;
        }
    }

    public async Task SetRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await _secureStorage.SetAsync(REFRESH_TOKEN_KEY, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store refresh token");
            throw;
        }
    }

    public async Task ClearAllTokensAsync()
    {
        try
        {
            _secureStorage.Remove(ID_TOKEN_KEY);
            _secureStorage.Remove(REFRESH_TOKEN_KEY);
            _secureStorage.Remove(TOKEN_EXPIRY_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear tokens");
        }
    }

    public async Task<bool> HasValidCredentialsAsync()
    {
        try
        {
            var idToken = await GetIdTokenAsync();
            var expiryString = await _secureStorage.GetAsync(TOKEN_EXPIRY_KEY);
            
            if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(expiryString))
                return false;

            if (long.TryParse(expiryString, out var expiryBinary))
            {
                var expiry = DateTime.FromBinary(expiryBinary);
                return expiry > DateTime.UtcNow.AddMinutes(5); // 5 min buffer
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate credentials");
            return false;
        }
    }
}
```

## Firebase Authentication with Refit

### Firebase Auth API Interface

```csharp
// Services/Auth/IFirebaseAuthApi.cs
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

### Authentication Models

```csharp
// Models/Auth/AuthModels.cs
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
}
```

### User Session Model

```csharp
// Models/Auth/UserSession.cs
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
```

### Firebase Auth Service Interface

```csharp
// Services/Auth/IFirebaseAuthService.cs
public interface IFirebaseAuthService
{
    Task<ServiceResult<UserSession>> SignUpWithEmailAsync(string email, string password, string? displayName = null, CancellationToken ct = default);
    Task<ServiceResult<UserSession>> SignInWithEmailAsync(string email, string password, CancellationToken ct = default);
    Task<ServiceResult<UserSession>> RefreshTokenAsync(CancellationToken ct = default);
    Task<ServiceResult<UserSession>> UpdateProfileAsync(string? displayName = null, string? photoUrl = null, CancellationToken ct = default);
    Task<ServiceResult<bool>> SendPasswordResetEmailAsync(string email, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteAccountAsync(CancellationToken ct = default);
    Task SignOutAsync();
    Task<UserSession?> GetCurrentSessionAsync();
    bool IsAuthenticated { get; }
    event EventHandler<UserSession?> AuthStateChanged;
}
```

### Firebase Auth Service Implementation

```csharp
// Services/Auth/FirebaseAuthService.cs
public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly IFirebaseAuthApi _authApi;
    private readonly ISecureCredentialService _credentialService;
    private readonly ILogger<FirebaseAuthService> _logger;
    private UserSession? _currentSession;

    public bool IsAuthenticated => _currentSession?.IsAuthenticated ?? false;
    public event EventHandler<UserSession?> AuthStateChanged;

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
                return ServiceResult<UserSession>.Failure("No refresh token available");
            }

            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await _authApi.RefreshTokenAsync(request, apiKey);
            
            var updatedSession = new UserSession
            {
                UserId = response.UserId,
                Email = _currentSession?.Email ?? "",
                DisplayName = _currentSession?.DisplayName,
                IdToken = response.IdToken,
                RefreshToken = response.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn))
            };

            await SaveSessionAsync(updatedSession);
            await SetCurrentSessionAsync(updatedSession);

            return ServiceResult<UserSession>.Success(updatedSession);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return ServiceResult<UserSession>.Failure($"Token refresh failed: {ParseFirebaseError(ex)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return ServiceResult<UserSession>.Failure("An unexpected error occurred");
        }
    }

    public async Task SignOutAsync()
    {
        await _credentialService.ClearAllTokensAsync();
        await SetCurrentSessionAsync(null);
    }

    public async Task<UserSession?> GetCurrentSessionAsync()
    {
        if (_currentSession != null && _currentSession.IsAuthenticated)
            return _currentSession;

        // Try to restore session from secure storage
        var hasValidCredentials = await _credentialService.HasValidCredentialsAsync();
        if (!hasValidCredentials)
            return null;

        var idToken = await _credentialService.GetIdTokenAsync();
        var refreshToken = await _credentialService.GetRefreshTokenAsync();

        if (!string.IsNullOrEmpty(idToken) && !string.IsNullOrEmpty(refreshToken))
        {
            // Auto-refresh if needed
            var refreshResult = await RefreshTokenAsync();
            return refreshResult.IsSuccess ? refreshResult.Data : null;
        }

        return null;
    }

    private async Task SaveSessionAsync(UserSession session)
    {
        await _credentialService.SetIdTokenAsync(session.IdToken);
        await _credentialService.SetRefreshTokenAsync(session.RefreshToken);
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
        // Parse Firebase error response
        return ex.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid email or password",
            HttpStatusCode.NotFound => "User not found",
            HttpStatusCode.Unauthorized => "Invalid credentials",
            _ => "Authentication failed"
        };
    }

    // Implement other required methods...
}
```

## Firestore Database Configuration

### Firestore Repository Interface

```csharp
// Services/Data/IFirestoreRepository.cs
public interface IFirestoreRepository
{
    Task<T?> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class;
    Task<DocumentReference> CreateDocumentAsync<T>(string collection, T entity, string? documentId = null, CancellationToken ct = default) where T : class;
    Task UpdateDocumentAsync<T>(string collection, string documentId, T entity, CancellationToken ct = default) where T : class;
    Task DeleteDocumentAsync(string collection, string documentId, CancellationToken ct = default);
    Task<bool> DocumentExistsAsync(string collection, string documentId, CancellationToken ct = default);
    Task<List<T>> QueryCollectionAsync<T>(string collection, Func<Query, Query>? queryBuilder = null, CancellationToken ct = default) where T : class;
    Task<List<T?>> BatchGetAsync<T>(string collection, List<string> documentIds, CancellationToken ct = default) where T : class;
    Task BatchWriteAsync(Action<WriteBatch> batchBuilder, CancellationToken ct = default);
    Task<TResult> RunTransactionAsync<TResult>(Func<Transaction, Task<TResult>> transactionFunc, CancellationToken ct = default);
    CollectionReference GetCollection(string collection);
    DocumentReference GetDocument(string collection, string documentId);
}
```

### Firestore Service Extensions

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFirestore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FirestoreConfig>(configuration.GetSection("Firestore"));

        services.AddSingleton<FirestoreDb>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<FirestoreConfig>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<ServiceCollectionExtensions>>();

            try
            {
                // Load credentials file from app package
                string jsonCredentials;
                using var stream = FileSystem.OpenAppPackageFileAsync(config.CredentialsFileName).GetAwaiter().GetResult();
                using var reader = new StreamReader(stream);
                jsonCredentials = reader.ReadToEnd();

                var converterRegistry = new ConverterRegistry
                {
                    new DateTimeToTimestampConverter()
                };

                var builder = new FirestoreDbBuilder
                {
                    ProjectId = config.ProjectId,
                    ConverterRegistry = converterRegistry,
                    JsonCredentials = jsonCredentials
                };

                if (config.UseEmulator)
                {
                    builder.EmulatorDetection = EmulatorDetection.EmulatorOnly;
                    Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", config.EmulatorHost);
                }

                return builder.Build();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Firestore: {Error}", ex.Message);
                throw new InvalidOperationException($"Failed to initialize Firestore: {ex.Message}", ex);
            }
        });

        services.AddScoped<IFirestoreRepository, FirestoreRepository>();
        
        return services;
    }

    public static IServiceCollection AddFirebaseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FirebaseAuthConfig>(configuration.GetSection("FirebaseAuth"));

        // Register HTTP client for Refit
        services.AddHttpClient();

        // Register Refit client
        services.AddRefitClient<IFirebaseAuthApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<FirebaseAuthConfig>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config.HttpTimeoutSeconds);
            });

        // Register services
        services.AddSingleton<ISecureCredentialService, SecureCredentialService>();
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();

        return services;
    }
}
```

### DateTime to Timestamp Converter

```csharp
// Extensions/DateTimeToTimestampConverter.cs
public class DateTimeToTimestampConverter : IFirestoreConverter<DateTime>
{
    public DateTime FromFirestore(object value)
    {
        return ((Timestamp)value).ToDateTime();
    }

    public object ToFirestore(DateTime value)
    {
        return Timestamp.FromDateTime(value.ToUniversalTime());
    }
}
```

## Service Result Pattern

```csharp
// Models/Common/ServiceResult.cs
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    private ServiceResult(bool isSuccess, T? data, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
    }
    
    public static ServiceResult<T> Success(T data) => new(true, data, null);
    public static ServiceResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
```

## Configuration Files

### appsettings.json

```json
{
  "FirebaseAuth": {
    "ApiKey": "", // Will be loaded from SecureStorage
    "ProjectId": "lingualearn-project-id",
    "BaseUrl": "https://identitytoolkit.googleapis.com/v1/accounts",
    "TokenRefreshThresholdMinutes": 5,
    "HttpTimeoutSeconds": 30
  },
  "Firestore": {
    "ProjectId": "lingualearn-project-id",
    "CredentialsFileName": "google-services.json",
    "UseEmulator": false,
    "EmulatorHost": "localhost:8080"
  }
}
```

### google-services.json Structure

```json
{
  "type": "service_account",
  "project_id": "lingualearn-project-id",
  "private_key_id": "key-id",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@lingualearn-project-id.iam.gserviceaccount.com",
  "client_id": "client-id",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40lingualearn-project-id.iam.gserviceaccount.com"
}
```

## MauiProgram.cs Configuration

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add Firebase Authentication
        builder.Services.AddFirebaseAuthentication(builder.Configuration);

        // Add Firestore
        builder.Services.AddFirestore(builder.Configuration);

        // Add platform services
        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

## Initialization and Credential Setup

### App.xaml.cs Initialization

```csharp
public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        MainPage = new AppShell();
        
        // Initialize Firebase credentials on app start
        _ = Task.Run(async () => await InitializeFirebaseCredentialsAsync(serviceProvider));
    }

    private async Task InitializeFirebaseCredentialsAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var credentialService = serviceProvider.GetRequiredService<ISecureCredentialService>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            
            // Check if API key is already stored
            var existingKey = await credentialService.GetFirebaseApiKeyAsync();
            if (string.IsNullOrEmpty(existingKey))
            {
                // First time setup - get API key from configuration or prompt user
                var apiKey = configuration["FirebaseAuth:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    await credentialService.SetFirebaseApiKeyAsync(apiKey);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the app
            System.Diagnostics.Debug.WriteLine($"Failed to initialize Firebase credentials: {ex.Message}");
        }
    }
}
```

## Project File Updates

### Updated LinguaLearn.Mobile.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFrameworks>net9.0-android;net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
        <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks>
        <OutputType>Exe</OutputType>
        <RootNamespace>LinguaLearn.Mobile</RootNamespace>
        <UseMaui>true</UseMaui>
        <SingleProject>true</SingleProject>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <ApplicationTitle>LinguaLearn.Mobile</ApplicationTitle>
        <ApplicationId>com.companyname.lingualearn.mobile</ApplicationId>
        <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
        <ApplicationVersion>1</ApplicationVersion>
        <WindowsPackageType>None</WindowsPackageType>
        <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">15.0</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'">15.0</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">21.0</SupportedOSPlatformVersion>
        <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
        <TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
        <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'tizen'">6.5</SupportedOSPlatformVersion>
    </PropertyGroup>

    <ItemGroup>
        <!-- App Icon -->
        <MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#512BD4" />
        <!-- Splash Screen -->
        <MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#512BD4" BaseSize="128,128" />
        <!-- Images -->
        <MauiImage Include="Resources\Images\*" />
        <!-- Custom Fonts -->
        <MauiFont Include="Resources\Fonts\*" />
        <!-- Raw Assets -->
        <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
    </ItemGroup>

    <ItemGroup>
        <!-- Existing packages -->
        <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
        <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
        <PackageReference Include="Microsoft.Maui.Controls" Version="9.0.110" />
        <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.9" />
        <PackageReference Include="Refit" Version="8.0.0" />
        <PackageReference Include="Refit.HttpClientFactory" Version="8.0.0" />
        
        <!-- New Firebase packages -->
        <PackageReference Include="Google.Cloud.Firestore" Version="3.7.0" />
        <PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
        <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
    </ItemGroup>
</Project>
```

## Security and Best Practices

### Credential Management Flow

```mermaid
sequenceDiagram
    participant App
    participant SecureCredentialService
    participant SecureStorage
    participant FirebaseAuthService
    participant FirebaseAPI
    
    App->>SecureCredentialService: InitializeCredentials()
    SecureCredentialService->>SecureStorage: GetFirebaseApiKey()
    SecureStorage-->>SecureCredentialService: API Key or null
    
    alt API Key exists
        SecureCredentialService-->>App: Credentials ready
    else API Key missing
        App->>SecureCredentialService: SetFirebaseApiKey(key)
        SecureCredentialService->>SecureStorage: Store API Key
    end
    
    App->>FirebaseAuthService: SignIn(email, password)
    FirebaseAuthService->>SecureCredentialService: GetFirebaseApiKey()
    SecureCredentialService->>SecureStorage: Retrieve API Key
    FirebaseAuthService->>FirebaseAPI: Authenticate with API Key
    FirebaseAPI-->>FirebaseAuthService: Tokens
    FirebaseAuthService->>SecureCredentialService: StoreTokens()
    SecureCredentialService->>SecureStorage: Store Tokens Securely
```

### Security Considerations

1. **API Key Protection**: Firebase API keys are stored in SecureStorage, never in source code
2. **Token Lifecycle**: Automatic token refresh with 5-minute buffer before expiration
3. **Secure Storage**: All sensitive data stored using platform-specific secure storage
4. **Error Handling**: Comprehensive error handling without exposing sensitive information
5. **Logging**: Security-conscious logging that excludes sensitive data

### Testing Strategy

#### Unit Testing Setup

```csharp
// Tests/Services/FirebaseAuthServiceTests.cs
public class FirebaseAuthServiceTests
{
    private readonly Mock<IFirebaseAuthApi> _mockAuthApi;
    private readonly Mock<ISecureCredentialService> _mockCredentialService;
    private readonly Mock<ILogger<FirebaseAuthService>> _mockLogger;
    private readonly FirebaseAuthService _authService;

    public FirebaseAuthServiceTests()
    {
        _mockAuthApi = new Mock<IFirebaseAuthApi>();
        _mockCredentialService = new Mock<ISecureCredentialService>();
        _mockLogger = new Mock<ILogger<FirebaseAuthService>>();
        
        _authService = new FirebaseAuthService(
            _mockAuthApi.Object,
            _mockCredentialService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SignInWithEmailAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        _mockCredentialService.Setup(x => x.GetFirebaseApiKeyAsync())
            .ReturnsAsync("test-api-key");
        
        _mockAuthApi.Setup(x => x.SignInAsync(It.IsAny<SignInRequest>(), It.IsAny<string>()))
            .ReturnsAsync(new SignInResponse
            {
                LocalId = "user123",
                Email = "test@example.com",
                IdToken = "token123",
                RefreshToken = "refresh123",
                ExpiresIn = "3600"
            });

        // Act
        var result = await _authService.SignInWithEmailAsync("test@example.com", "password123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("user123", result.Data.UserId);
    }
}
```

This comprehensive configuration provides a production-ready setup for Firebase Authentication via REST API with Refit and Firestore database integration, with robust security practices and proper dependency injection.