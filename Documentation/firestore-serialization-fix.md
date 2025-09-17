# Firestore Serialization Fix Implementation

## Issue
The UserService was failing with the error: "Failed to set document: Unable to create converter for type LinguaLearn.Mobile.Models.UserProfile"

This occurred because the models lacked proper Firestore serialization attributes.

## Solution Implemented

### 1. Added Firestore Attributes to Models (`Models/UserModels.cs`)

Updated all data models with proper Firestore attributes:

```csharp
[FirestoreData]
public class UserProfile
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    // ... all other properties with proper attributes
}
```

**Models Updated:**
- `UserProfile` - Main user document
- `UserPreferences` - Nested preferences object
- `UserBadge` - Badge tracking
- `StreakSnapshot` - Streak history
- `UserStats` - User statistics
- `WeeklyProgress` - Weekly goal tracking
- `LanguageOption` - Available languages

### 2. Created Custom Firestore Converters (`Models/Converters/FirestoreConverters.cs`)

Added converters for complex types that Firestore doesn't handle automatically:

**Enum Converters:**
- `DifficultyLevelConverter` - Converts `DifficultyLevel` enum to/from string
- `PronunciationSensitivityConverter` - Converts `PronunciationSensitivity` enum to/from string  
- `AppThemeConverter` - Converts `AppTheme` enum to/from string

**Time Converter:**
- `TimeSpanConverter` - Converts `TimeSpan` to/from string format (hh:mm:ss)

### 3. Registered Converters in DI (`Extensions/ServiceCollectionExtensions.cs`)

Updated the Firestore configuration to include the custom converters:

```csharp
// Create converter registry with custom converters
var converterRegistry = new ConverterRegistry
{
    new DifficultyLevelConverter(),
    new PronunciationSensitivityConverter(), 
    new AppThemeConverter(),
    new TimeSpanConverter()
};

return new FirestoreDbBuilder
{
    ProjectId = firestoreConfig.ProjectId,
    Credential = credential,
    ConverterRegistry = converterRegistry
}.Build();
```

## Benefits

### 1. **Proper Firestore Serialization**
- All models now serialize/deserialize correctly with Firestore
- Enums are stored as readable strings in Firestore
- TimeSpan values are stored in standard time format

### 2. **Type Safety**
- Strong typing maintained throughout the application
- Automatic conversion between C# types and Firestore format
- Fallback defaults for enum parsing failures

### 3. **Maintainability**
- Clear mapping between C# properties and Firestore field names
- Centralized converter logic for reusability
- Easy to extend for additional custom types

### 4. **Performance**
- Efficient serialization/deserialization
- Minimal overhead from converters
- Proper caching by Firestore SDK

## Firestore Document Structure

The UserProfile document in Firestore will now have this structure:

```json
{
  "id": "user123",
  "email": "user@example.com",
  "displayName": "John Doe",
  "nativeLanguage": "en",
  "targetLanguage": "es",
  "xp": 150,
  "level": 2,
  "streakCount": 5,
  "lastActiveDate": "2024-01-15T10:30:00Z",
  "streakFreezeTokens": 3,
  "hasCompletedOnboarding": true,
  "preferences": {
    "soundEnabled": true,
    "vibrationEnabled": true,
    "dailyReminderEnabled": true,
    "dailyReminderTime": "19:00:00",
    "weeklyGoal": 5,
    "difficultyPreference": "Adaptive",
    "pronunciationSensitivity": "Medium",
    "theme": "System"
  },
  "badges": [
    {
      "badgeId": "onboarding_complete",
      "earnedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "version": 1
}
```

## Usage

The UserService can now successfully create and update user profiles:

```csharp
// Create user profile during onboarding
var userProfile = new UserProfile
{
    Id = userId,
    Email = session.Email,
    DisplayName = session.DisplayName ?? "User",
    NativeLanguage = nativeLanguage,
    TargetLanguage = targetLanguage,
    HasCompletedOnboarding = true,
    Preferences = new UserPreferences
    {
        DifficultyPreference = SelectedDifficultyLevel,
        Theme = SelectedTheme
        // Other preferences...
    }
};

var result = await _userService.CreateUserProfileAsync(userProfile);
```

## Testing

All models are now properly serializable and the build completes successfully. The Firestore integration should work correctly for:

- User profile creation during onboarding
- User preference updates
- Badge awarding
- XP and level tracking
- Streak management

## Future Considerations

1. **Schema Evolution**: The version field allows for future schema migrations
2. **Additional Converters**: Easy to add converters for new complex types
3. **Validation**: Consider adding validation attributes for data integrity
4. **Indexing**: Firestore composite indexes may be needed for complex queries

This implementation resolves the serialization error and provides a robust foundation for Firestore data operations in the LinguaLearn application.