# Firebase Authentication and Firestore Setup - Usage Guide

## Overview

This document provides instructions on how to use the Firebase Authentication and Firestore services implemented in the LinguaLearn MAUI application.

## Configuration Setup

### 1. Firebase Project Setup

1. Create a Firebase project at [Firebase Console](https://console.firebase.google.com/)
2. Enable Authentication and Firestore Database
3. Create a service account for Firestore access:
   - Go to Project Settings > Service Accounts
   - Click "Generate new private key"
   - Download the JSON file
4. Get your Firebase configuration details:
   - API Key (for Authentication)
   - Project ID

### 2. Update Configuration

1. **Replace service account credentials**: Replace the content of `Resources/Raw/google-services.json` with your downloaded service account JSON file.

2. **Update app settings**: Update the `Resources/Raw/appsettings.json` file with your Firebase credentials:

```json
{
  "Firebase": {
    "Auth": {
      "ApiKey": "YOUR_ACTUAL_FIREBASE_API_KEY",
      "ProjectId": "YOUR_ACTUAL_PROJECT_ID",
      "BaseUrl": "https://identitytoolkit.googleapis.com/v1/accounts",
      "TokenRefreshThresholdMinutes": 5,
      "HttpTimeoutSeconds": 30
    },
    "Firestore": {
      "ProjectId": "YOUR_ACTUAL_PROJECT_ID",
      "CredentialsFileName": "google-services.json",
      "UseEmulator": false,
      "EmulatorHost": "localhost:8080"
    }
  }
}
```

### 3. Service Registration

Services are automatically registered in `MauiProgram.cs`. No additional setup required.

## Usage Examples

### Authentication Service

```csharp
// Inject the service in your page/view model
public partial class LoginViewModel : ObservableObject
{
    private readonly IFirebaseAuthService _authService;

    public LoginViewModel(IFirebaseAuthService authService)
    {
        _authService = authService;
        
        // Subscribe to auth state changes
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    // Sign up a new user
    public async Task<bool> SignUpAsync(string email, string password, string displayName)
    {
        var result = await _authService.SignUpWithEmailAsync(email, password, displayName);
        
        if (result.IsSuccess)
        {
            // User successfully created
            var userSession = result.Data;
            return true;
        }
        else
        {
            // Handle error
            Console.WriteLine($"Sign up failed: {result.ErrorMessage}");
            return false;
        }
    }

    // Sign in existing user
    public async Task<bool> SignInAsync(string email, string password)
    {
        var result = await _authService.SignInWithEmailAsync(email, password);
        
        if (result.IsSuccess)
        {
            var userSession = result.Data;
            return true;
        }
        else
        {
            Console.WriteLine($"Sign in failed: {result.ErrorMessage}");
            return false;
        }
    }

    // Sign out
    public async Task SignOutAsync()
    {
        await _authService.SignOutAsync();
    }

    // Check if user is authenticated
    public bool IsAuthenticated => _authService.IsAuthenticated;

    // Get current user session
    public async Task<UserSession?> GetCurrentUserAsync()
    {
        return await _authService.GetCurrentSessionAsync();
    }

    private void OnAuthStateChanged(object? sender, UserSession? session)
    {
        // Handle authentication state changes
        if (session != null)
        {
            // User is signed in
            Console.WriteLine($"User signed in: {session.Email}");
        }
        else
        {
            // User is signed out
            Console.WriteLine("User signed out");
        }
    }
}
```

### Firestore Repository

```csharp
// Define your data models
[FirestoreData]
public class UserProfile
{
    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string DisplayName { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public string Email { get; set; } = string.Empty;
    
    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }
    
    [FirestoreProperty]
    public DateTime LastLoginAt { get; set; }
}

// Use the repository in your service/view model
public class UserProfileService
{
    private readonly IFirestoreRepository _repository;

    public UserProfileService(IFirestoreRepository repository)
    {
        _repository = repository;
    }

    // Create user profile
    public async Task<bool> CreateUserProfileAsync(UserProfile profile)
    {
        var result = await _repository.SetDocumentAsync("users", profile.UserId, profile);
        return result.IsSuccess;
    }

    // Get user profile
    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        var result = await _repository.GetDocumentAsync<UserProfile>("users", userId);
        return result.IsSuccess ? result.Data : null;
    }

    // Update user profile
    public async Task<bool> UpdateUserProfileAsync(string userId, Dictionary<string, object> updates)
    {
        var result = await _repository.UpdateDocumentAsync("users", userId, updates);
        return result.IsSuccess;
    }

    // Query users
    public async Task<List<UserProfile>> GetUsersByEmailDomainAsync(string domain)
    {
        var filter = Filter.GreaterThanOrEqualTo("email", domain)
                          .And(Filter.LessThan("email", domain + "\uf8ff"));
        
        var result = await _repository.QueryCollectionAsync<UserProfile>("users", filter);
        return result.IsSuccess ? result.Data! : new List<UserProfile>();
    }

    // Delete user profile
    public async Task<bool> DeleteUserProfileAsync(string userId)
    {
        var result = await _repository.DeleteDocumentAsync("users", userId);
        return result.IsSuccess;
    }
}
```

## Security Considerations

1. **API Key Storage**: The Firebase API key is stored securely using MAUI's `ISecureStorage`
2. **Token Management**: ID tokens and refresh tokens are automatically managed and stored securely
3. **Token Refresh**: Tokens are automatically refreshed when they expire
4. **Error Handling**: All operations return `ServiceResult<T>` for consistent error handling

## Firestore Security Rules

Make sure to configure appropriate Firestore security rules in your Firebase console:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Users can only access their own data
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Add other collection rules as needed
  }
}
```

## Testing

For development and testing, you can use the Firestore emulator:

1. Set `UseEmulator: true` in your configuration
2. Start the Firestore emulator: `firebase emulators:start --only firestore`
3. The app will automatically connect to the local emulator

## Troubleshooting

### Common Issues

1. **Build Errors**: Ensure all NuGet packages are restored
2. **Authentication Fails**: Verify your Firebase API key and project ID are correct
3. **Firestore Connection**: Check your internet connection and Firebase project settings
4. **Token Expiry**: The service automatically handles token refresh, but check logs for any refresh failures

### Logging

Enable debug logging to troubleshoot issues:

```csharp
// In MauiProgram.cs
#if DEBUG
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
```

## Next Steps

1. **Setup Firebase Project**:
   - Create a Firebase project
   - Enable Authentication and Firestore
   - Download service account JSON file

2. **Configure Credentials**:
   - Replace `Resources/Raw/google-services.json` with your service account file
   - Update `Resources/Raw/appsettings.json` with your Firebase API key and project ID

3. **Implementation**:
   - Implement user interface components for authentication
   - Create additional Firestore data models as needed
   - Set up appropriate Firestore security rules
   - Test the implementation thoroughly

## Important Notes

- The service account JSON file (`google-services.json`) is used for Firestore authentication
- The Firebase API key in `appsettings.json` is used for Firebase Authentication
- Both files should be kept secure and not committed to version control with real credentials
- For production deployment, consider using secure configuration management

## Support

For Firebase-specific issues, refer to:
- [Firebase Documentation](https://firebase.google.com/docs)
- [Firestore .NET Client Documentation](https://cloud.google.com/firestore/docs/client-libraries)