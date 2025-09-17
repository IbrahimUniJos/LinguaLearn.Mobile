# User Model System - LinguaLearn MAUI

## Overview

This document describes the comprehensive user model system created for the LinguaLearn MAUI application, following the specifications in `maui-app-specs.md` and Firestore best practices.

## Architecture

The user model system follows the simplified MVVM architecture specified in the app specs, with clear separation of concerns:

- **Models**: Plain POCOs with JSON serialization attributes for Firestore
- **Services**: Business logic interfaces and implementations
- **Gamification**: XP, Streaks, and Badge systems as separate services

## Core Models

### 1. UserProfile (Main Document)
**Firestore Collection**: `users/{userId}`

```csharp
public class UserProfile
{
    public string Id { get; set; }                    // User ID from Firebase Auth
    public string Email { get; set; }                 // User email
    public string DisplayName { get; set; }           // Display name
    public string? NativeLanguage { get; set; }       // User's native language
    public string? TargetLanguage { get; set; }       // Language being learned
    
    // Gamification Stats (denormalized for quick access)
    public int XP { get; set; } = 0;                  // Total XP earned
    public int Level { get; set; } = 1;               // Current level
    public int StreakCount { get; set; } = 0;         // Current streak
    public DateTime? LastActiveDate { get; set; }     // Last activity date
    public int StreakFreezeTokens { get; set; } = 0;  // Streak freeze tokens
    
    // User State
    public bool HasCompletedOnboarding { get; set; } = false;
    public UserPreferences Preferences { get; set; } = new();
    public List<UserBadge> Badges { get; set; } = new();
    
    // Metadata (following Firestore best practices)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;             // For conflict resolution
}
```

### 2. UserPreferences (Nested Object)
User preferences embedded within the UserProfile document:

```csharp
public class UserPreferences
{
    public bool SoundEnabled { get; set; } = true;
    public bool VibrationEnabled { get; set; } = true;
    public bool DailyReminderEnabled { get; set; } = true;
    public TimeSpan DailyReminderTime { get; set; } = new(19, 0, 0);
    public int WeeklyGoal { get; set; } = 5;
    public DifficultyLevel DifficultyPreference { get; set; } = DifficultyLevel.Adaptive;
    public PronunciationSensitivity PronunciationSensitivity { get; set; } = PronunciationSensitivity.Medium;
    public AppTheme Theme { get; set; } = AppTheme.System;
}
```

### 3. StreakSnapshot (Subcollection)
**Firestore Collection**: `users/{userId}/streaks/{snapshotId}`

Detailed streak tracking for analytics and history:

```csharp
public class StreakSnapshot
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CurrentCount { get; set; }
    public int MaxCount { get; set; }
    public bool IsActive { get; set; } = true;
    public int FreezeTokensUsed { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 4. UserStats (Document)
**Firestore Collection**: `users/{userId}/stats/current`

Aggregated statistics computed from progress records:

```csharp
public class UserStats
{
    public string UserId { get; set; }
    public int TotalLessonsCompleted { get; set; } = 0;
    public int TotalQuizzesCompleted { get; set; } = 0;
    public int TotalXPEarned { get; set; } = 0;
    public double AverageQuizAccuracy { get; set; } = 0.0;
    public int TotalStudyTimeMinutes { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public int BadgesEarned { get; set; } = 0;
    public WeeklyProgress CurrentWeek { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
```

### 5. UserSession (In-Memory Only)
Used for application state management, not stored in Firestore:

```csharp
public class UserSession
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string? DisplayName { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken) && DateTime.UtcNow < ExpiresAt;
}
```

## Service Interfaces

### 1. IUserService
Main service for user profile management:

```csharp
public interface IUserService
{
    // Profile Management
    Task<ServiceResult<UserProfile>> GetUserProfileAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<UserProfile>> CreateUserProfileAsync(UserProfile profile, CancellationToken ct = default);
    Task<ServiceResult<UserProfile>> UpdateUserProfileAsync(UserProfile profile, CancellationToken ct = default);
    
    // Onboarding
    Task<ServiceResult<bool>> CompleteOnboardingAsync(string userId, string nativeLanguage, string targetLanguage, CancellationToken ct = default);
    
    // Gamification
    Task<ServiceResult<int>> AddXPAsync(string userId, int xpAmount, string reason, CancellationToken ct = default);
    Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default);
    
    // Statistics
    Task<ServiceResult<UserStats>> GetUserStatsAsync(string userId, CancellationToken ct = default);
}
```

### 2. IXPService
XP calculations and level management following the specified formula:

```csharp
public interface IXPService
{
    // XP Calculations
    int CalculateLessonXP(string lessonDifficulty, TimeSpan completionTime, double accuracy, int streakCount);
    int CalculateQuizXP(int questionsCorrect, int totalQuestions, string difficulty, int streakCount);
    
    // Level Calculations (Formula: 50 * level^1.7)
    int CalculateXPRequiredForLevel(int level);
    int CalculateCurrentLevel(int totalXP);
    int CalculateXPForNextLevel(int totalXP);
    double CalculateProgressPercentageInCurrentLevel(int totalXP);
}
```

### 3. IStreakService
Streak management with midnight boundary and grace period logic:

```csharp
public interface IStreakService
{
    // Core streak operations
    Task<ServiceResult<int>> UpdateStreakAsync(string userId, DateTime activityDate, CancellationToken ct = default);
    Task<ServiceResult<StreakSnapshot>> GetCurrentStreakAsync(string userId, CancellationToken ct = default);
    
    // Streak freeze functionality
    Task<ServiceResult<bool>> UseStreakFreezeAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<bool>> AwardStreakFreezeTokenAsync(string userId, string reason, CancellationToken ct = default);
    
    // Streak calculations
    bool IsStreakBroken(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone);
    bool IsWithinGracePeriod(DateTime lastActiveDate, DateTime currentDate, TimeZoneInfo userTimeZone);
}
```

### 4. IBadgeService
Event-driven badge awarding system:

```csharp
public interface IBadgeService
{
    // Badge awarding
    Task<ServiceResult<bool>> AwardBadgeAsync(string userId, string badgeId, string reason, CancellationToken ct = default);
    Task<ServiceResult<bool>> CheckAndAwardAsync(string userId, string eventType, object eventData, CancellationToken ct = default);
    
    // Badge queries
    Task<ServiceResult<List<UserBadge>>> GetUserBadgesAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<List<BadgeDefinition>>> GetAllBadgeDefinitionsAsync(CancellationToken ct = default);
}
```

## Firestore Best Practices Implemented

### 1. Flat Document Structure
- Main user data in a single document for atomic updates
- Subcollections for historical data (streaks)
- Denormalized badge list for quick access

### 2. Naming Conventions
- camelCase property names for Firestore compatibility
- Consistent naming throughout the system
- Clear, descriptive property names

### 3. Metadata Fields
- `createdAt` and `updatedAt` timestamps (UTC)
- `version` field for conflict resolution
- `isActive` flags for soft deletes

### 4. Data Types
- UTC timestamps for all dates
- Proper boolean flags
- Structured nested objects for preferences

### 5. Collection Organization
```
users/
  {userId}/
    - UserProfile document
    
    streaks/
      {snapshotId}/
        - StreakSnapshot document
    
    stats/
      current/
        - UserStats document

badges/
  definitions/
    {badgeId}/
      - BadgeDefinition document
```

## Gamification System

### XP Calculation Formula
As specified in the app specs:
- **Base XP**: Activity type dependent (20 for lessons, 10 for quiz questions, 15 for pronunciation)
- **Difficulty Multiplier**: 1x (beginner), 2x (intermediate), 3x (advanced), 4x (expert)
- **Streak Bonus**: Progressive bonus based on streak count
- **Accuracy Multiplier**: 0.6x (poor) to 1.5x (perfect)

### Level Curve
Quadratic/exponential hybrid as specified:
- **Formula**: `XP Required = 50 * level^1.7`
- **Progressive difficulty**: Higher levels require exponentially more XP

### Streak Logic
- **Midnight Boundary**: Streaks reset at midnight in user's timezone
- **Grace Period**: 4-hour grace period after midnight
- **Freeze Tokens**: Allow users to maintain streaks when they can't practice

### Badge System
- **Event-Driven**: Badges awarded based on specific events (lesson completion, streak milestones, etc.)
- **Categories**: Lessons, Streaks, Quizzes, Pronunciation, Social, Milestones, Achievements, Special
- **Rarity Levels**: Common, Uncommon, Rare, Epic, Legendary

## Language Support

### Available Languages
The system includes support for 10 languages with proper localization:

```csharp
public static class AvailableLanguages
{
    public static readonly List<LanguageOption> All = new()
    {
        new() { Code = "en", Name = "English", NativeName = "English", FlagEmoji = "????" },
        new() { Code = "es", Name = "Spanish", NativeName = "Español", FlagEmoji = "????" },
        new() { Code = "fr", Name = "French", NativeName = "Français", FlagEmoji = "????" },
        // ... additional languages
    };
}
```

## Integration with Firebase

### Authentication Integration
- User ID comes from Firebase Authentication
- Profile created after successful authentication
- Secure token management through `UserSession`

### Firestore Security Rules
Recommended security rules for the user system:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Users can read/write their own profile
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
      
      // Allow read access to public profile fields for leaderboards
      match /public {
        allow read: if request.auth != null;
      }
    }
    
    // Badge definitions are read-only for all authenticated users
    match /badges/definitions/{badgeId} {
      allow read: if request.auth != null;
    }
  }
}
```

## Performance Considerations

### 1. Denormalization
- Badge list stored directly in user profile for quick access
- Current streak count cached in main profile document
- Statistics aggregated and cached

### 2. Query Optimization
- Indexed fields for common queries (level, XP, streak count)
- Minimal data fetching with projection queries where possible
- Batch operations for multiple user updates

### 3. Offline Support
- All models work with Firestore's offline capabilities
- Conflict resolution through version fields
- Queue-based sync for progress updates

## Testing Strategy

### Unit Tests
- Service interfaces easily mockable
- XP calculation logic testable in isolation
- Streak logic testable with fixed time scenarios

### Integration Tests
- Firestore operations with emulator
- End-to-end user flow testing
- Performance testing with realistic data volumes

## Future Enhancements

### Planned Features
1. **Social Features**: Friend lists, challenges between users
2. **Advanced Analytics**: Learning pattern analysis, difficulty adaptation
3. **Achievements**: Complex multi-step achievements
4. **Seasonal Events**: Time-limited challenges and rewards

### Scalability Considerations
1. **Sharding**: User data can be sharded by region if needed
2. **Caching**: Redis layer for frequently accessed data
3. **Batch Processing**: Background jobs for statistics aggregation
4. **Archive Strategy**: Old streak snapshots moved to cold storage

This user model system provides a solid foundation for the LinguaLearn application, following the specifications while implementing Firestore best practices for performance, scalability, and maintainability.