# Main Game Logic Implementation Plan

## Overview
This document outlines the implementation plan for the main game logic of the LinguaLearn mobile application. The implementation will include real-time Firestore operations for game state management, using Material 3 design principles with Horus Material controls for a modern, interactive UI.

## Core Game Components

### 1. Lesson System
- **Lesson Structure**: Hierarchical organization with modules, lessons, and sections
- **Content Types**: Vocabulary, grammar, pronunciation, and quizzes
- **Progress Tracking**: Real-time tracking of user progress through lessons
- **Adaptive Difficulty**: Content difficulty adjusts based on user performance

### 2. Quiz Engine
- **Question Types**: Multiple choice, fill in the blank, matching, translation, listening, and speaking
- **Scoring System**: Real-time scoring with accuracy tracking
- **Adaptive Algorithm**: Difficulty adjusts based on response time and accuracy
- **Review System**: Incorrect answers are revisited in spaced repetition

### 3. Gamification System
- **XP System**: Experience points earned through activities
- **Level Progression**: Quadratic/exponential level curve (Level XP = 50 * level^1.7)
- **Streak Tracking**: Daily streaks with midnight boundary and grace periods
- **Badge System**: Achievement badges for milestones and accomplishments
- **Leaderboards**: Weekly and all-time leaderboards

### 4. Pronunciation Practice
- **Speech Recognition**: Platform-native speech-to-text implementation
- **Pronunciation Scoring**: Heuristic scoring based on phoneme comparison
- **Audio Feedback**: Visual and auditory feedback for pronunciation practice
- **Cloud Storage**: Audio clips stored in Firebase Cloud Storage

## Architecture Design

### Data Models

#### Lesson Models
```csharp
[FirestoreData]
public class Lesson
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("language")]
    public string Language { get; set; } = string.Empty;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [FirestoreProperty("order")]
    public int Order { get; set; }

    [FirestoreProperty("prerequisites")]
    public List<string> Prerequisites { get; set; } = new();

    [FirestoreProperty("sections")]
    public List<LessonSection> Sections { get; set; } = new();

    [FirestoreProperty("estimatedDurationMinutes")]
    public int EstimatedDurationMinutes { get; set; }

    [FirestoreProperty("xpReward")]
    public int XPReward { get; set; }

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

[FirestoreData]
public class LessonSection
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "vocabulary", "grammar", "pronunciation", "quiz"

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("content")]
    public string Content { get; set; } = string.Empty;

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("order")]
    public int Order { get; set; }
}
```

#### Quiz Models
```csharp
[FirestoreData]
public class Quiz
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("questions")]
    public List<QuizQuestion> Questions { get; set; } = new();

    [FirestoreProperty("timeLimit")]
    public int TimeLimit { get; set; }

    [FirestoreProperty("passingScore")]
    public int PassingScore { get; set; }

    [FirestoreProperty("adaptiveSettings")]
    public AdaptiveConfig AdaptiveSettings { get; set; } = new();

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;
}

[FirestoreData]
public class QuizQuestion
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty; // "multiple_choice", "fill_blank", "matching", etc.

    [FirestoreProperty("question")]
    public string Question { get; set; } = string.Empty;

    [FirestoreProperty("options")]
    public List<string> Options { get; set; } = new();

    [FirestoreProperty("correctAnswers")]
    public List<string> CorrectAnswers { get; set; } = new();

    [FirestoreProperty("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [FirestoreProperty("points")]
    public int Points { get; set; } = 1;

    [FirestoreProperty("difficulty")]
    public string Difficulty { get; set; } = "medium";

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

[FirestoreData]
public class AdaptiveConfig
{
    [FirestoreProperty("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    [FirestoreProperty("difficultyAdjustmentFactor")]
    public double DifficultyAdjustmentFactor { get; set; } = 0.1;

    [FirestoreProperty("minQuestionsPerSession")]
    public int MinQuestionsPerSession { get; set; } = 5;

    [FirestoreProperty("maxQuestionsPerSession")]
    public int MaxQuestionsPerSession { get; set; } = 20;
}
```

#### Progress Models
```csharp
[FirestoreData]
public class ProgressRecord
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("lessonId")]
    public string LessonId { get; set; } = string.Empty;

    [FirestoreProperty("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [FirestoreProperty("score")]
    public double Score { get; set; }

    [FirestoreProperty("accuracy")]
    public double Accuracy { get; set; }

    [FirestoreProperty("timeSpentSeconds")]
    public int TimeSpentSeconds { get; set; }

    [FirestoreProperty("xpEarned")]
    public int XPEarned { get; set; }

    [FirestoreProperty("isCompleted")]
    public bool IsCompleted { get; set; }

    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [FirestoreProperty("completedAt")]
    public DateTime CompletedAt { get; set; }
}
```

## Service Layer Implementation

### ILessonService
```csharp
public interface ILessonService
{
    // Lesson Operations
    Task<ServiceResult<List<Lesson>>> GetLessonsAsync(CancellationToken ct = default);
    Task<ServiceResult<Lesson?>> GetLessonAsync(string lessonId, CancellationToken ct = default);
    Task<ServiceResult<Lesson>> CreateLessonAsync(Lesson lesson, CancellationToken ct = default);
    Task<ServiceResult<Lesson>> UpdateLessonAsync(Lesson lesson, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteLessonAsync(string lessonId, CancellationToken ct = default);
    
    // Lesson Progress
    Task<ServiceResult<bool>> StartLessonAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<bool>> CompleteLessonAsync(string userId, string lessonId, int xpEarned, CancellationToken ct = default);
    Task<ServiceResult<UserProgress?>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    
    // Prerequisites
    Task<ServiceResult<bool>> ArePrerequisitesMetAsync(string userId, string lessonId, CancellationToken ct = default);
    
    // Recommendations
    Task<ServiceResult<List<Lesson>>> GetRecommendedLessonsAsync(string userId, int count = 5, CancellationToken ct = default);
}
```

### IQuizService
```csharp
public interface IQuizService
{
    // Quiz Operations
    Task<ServiceResult<Quiz?>> GetQuizAsync(string quizId, CancellationToken ct = default);
    Task<ServiceResult<Quiz>> CreateQuizAsync(Quiz quiz, CancellationToken ct = default);
    Task<ServiceResult<Quiz>> UpdateQuizAsync(Quiz quiz, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteQuizAsync(string quizId, CancellationToken ct = default);
    
    // Quiz Session Management
    Task<ServiceResult<QuizSession>> StartQuizSessionAsync(string userId, string quizId, CancellationToken ct = default);
    Task<ServiceResult<QuizSession>> SubmitAnswerAsync(string sessionId, string questionId, List<string> answers, CancellationToken ct = default);
    Task<ServiceResult<QuizResult>> CompleteQuizSessionAsync(string sessionId, CancellationToken ct = default);
    
    // Adaptive Engine
    Task<ServiceResult<QuizQuestion?>> GetNextQuestionAsync(string sessionId, CancellationToken ct = default);
    Task<ServiceResult<double>> CalculateAdaptiveDifficultyAsync(string userId, string skillId, CancellationToken ct = default);
    
    // History
    Task<ServiceResult<List<QuizResult>>> GetUserQuizHistoryAsync(string userId, int limit = 10, CancellationToken ct = default);
}
```

### IProgressService
```csharp
public interface IProgressService
{
    // Progress Tracking
    Task<ServiceResult<ProgressRecord>> RecordProgressAsync(ProgressRecord record, CancellationToken ct = default);
    Task<ServiceResult<List<ProgressRecord>>> GetUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    Task<ServiceResult<UserProgress>> CalculateUserProgressAsync(string userId, string lessonId, CancellationToken ct = default);
    
    // Analytics
    Task<ServiceResult<double>> CalculateUserAccuracyAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<TimeSpan>> CalculateTotalStudyTimeAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<int>> CalculateCompletedLessonsAsync(string userId, CancellationToken ct = default);
    
    // Review System
    Task<ServiceResult<List<ReviewItem>>> GetReviewItemsAsync(string userId, int count = 10, CancellationToken ct = default);
    Task<ServiceResult<bool>> ScheduleReviewAsync(string userId, string itemId, DateTime reviewDate, CancellationToken ct = default);
}
```

## Real-time Firestore Integration

### Real-time Listeners
```csharp
public class RealtimeLessonService : ILessonService
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly Dictionary<string, CancellationTokenSource> _listeners;
    
    public async IAsyncEnumerable<Lesson> ListenToLessonsAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var lesson in _firestoreRepository.ListenCollectionAsync<Lesson>("lessons", ct))
        {
            yield return lesson;
        }
    }
    
    public async Task<IDisposable> ListenToUserProgressAsync(
        string userId, 
        string lessonId, 
        Action<UserProgress> onProgressUpdate,
        CancellationToken ct = default)
    {
        return await _firestoreRepository.ListenToDocumentAsync<UserProgress>(
            $"users/{userId}/progress/{lessonId}",
            progress => {
                if (progress != null) onProgressUpdate(progress);
            },
            ct);
    }
}
```

### Transactional Updates
```csharp
public async Task<ServiceResult<bool>> CompleteLessonWithGamificationAsync(
    string userId, 
    string lessonId, 
    int xpEarned, 
    CancellationToken ct = default)
{
    try
    {
        return await _firestoreRepository.RunTransactionAsync(async transaction => {
            // 1. Update user profile with XP and streak
            var userProfile = await transaction.GetDocumentAsync<UserProfile>($"users/{userId}");
            if (userProfile == null) return false;
            
            userProfile.XP += xpEarned;
            userProfile.Level = _xpService.CalculateCurrentLevel(userProfile.XP);
            userProfile.LastActiveDate = DateTime.UtcNow;
            
            // Update streak if this is the first lesson of the day
            if (ShouldUpdateStreak(userProfile.LastActiveDate))
            {
                userProfile.StreakCount++;
            }
            
            await transaction.SetDocumentAsync($"users/{userId}", userProfile);
            
            // 2. Record progress
            var progressRecord = new ProgressRecord {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                LessonId = lessonId,
                Score = 1.0,
                Accuracy = 1.0,
                TimeSpentSeconds = 0, // Would be calculated from start/end times
                XPEarned = xpEarned,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            
            await transaction.SetDocumentAsync(
                $"users/{userId}/progress/{progressRecord.Id}", 
                progressRecord);
            
            // 3. Record activity
            var activity = ActivityHelper.CreateLessonCompletedActivity(
                userId, 
                "Lesson Title", // Would get actual lesson title
                xpEarned);
                
            await transaction.SetDocumentAsync(
                $"users/{userId}/activities/{activity.Id}", 
                activity);
                
            return true;
        }, ct);
    }
    catch (Exception ex)
    {
        return ServiceResult<bool>.Failure($"Failed to complete lesson: {ex.Message}");
    }
}
```

## ViewModel Implementation

### LessonPlayerViewModel
```csharp
public partial class LessonPlayerViewModel : ObservableObject
{
    private readonly ILessonService _lessonService;
    private readonly IProgressService _progressService;
    private readonly IUserService _userService;
    private readonly IAudioService _audioService;
    
    [ObservableProperty]
    private Lesson? _currentLesson;
    
    [ObservableProperty]
    private LessonSection? _currentSection;
    
    [ObservableProperty]
    private int _currentSectionIndex;
    
    [ObservableProperty]
    private bool _isPlayingAudio;
    
    [ObservableProperty]
    private string? _userAnswer;
    
    [ObservableProperty]
    private bool _isAnswerCorrect;
    
    [ObservableProperty]
    private int _xpEarned;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public ObservableCollection<LessonSection> Sections { get; } = new();
    
    public LessonPlayerViewModel(
        ILessonService lessonService,
        IProgressService progressService,
        IUserService userService,
        IAudioService audioService)
    {
        _lessonService = lessonService;
        _progressService = progressService;
        _userService = userService;
        _audioService = audioService;
    }
    
    public async Task InitializeAsync(string lessonId)
    {
        try
        {
            IsLoading = true;
            var result = await _lessonService.GetLessonAsync(lessonId);
            
            if (result.IsSuccess && result.Data != null)
            {
                CurrentLesson = result.Data;
                Sections.Clear();
                
                foreach (var section in CurrentLesson.Sections.OrderBy(s => s.Order))
                {
                    Sections.Add(section);
                }
                
                if (Sections.Any())
                {
                    await NavigateToSectionAsync(0);
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to load lesson";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading lesson: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task NavigateToSectionAsync(int sectionIndex)
    {
        if (sectionIndex >= 0 && sectionIndex < Sections.Count)
        {
            CurrentSectionIndex = sectionIndex;
            CurrentSection = Sections[sectionIndex];
            
            // Auto-play audio for pronunciation sections
            if (CurrentSection.Type == "pronunciation")
            {
                await PlaySectionAudioAsync();
            }
        }
    }
    
    [RelayCommand]
    private async Task PlaySectionAudioAsync()
    {
        if (CurrentSection?.Metadata.ContainsKey("audioUrl") == true)
        {
            try
            {
                IsPlayingAudio = true;
                var audioUrl = CurrentSection.Metadata["audioUrl"].ToString();
                if (!string.IsNullOrEmpty(audioUrl))
                {
                    await _audioService.PlayAudioAsync(audioUrl);
                }
            }
            finally
            {
                IsPlayingAudio = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task SubmitAnswerAsync()
    {
        if (string.IsNullOrWhiteSpace(UserAnswer) || CurrentSection == null)
            return;
            
        try
        {
            IsLoading = true;
            
            // Validate answer based on section type
            var isCorrect = ValidateAnswer(UserAnswer, CurrentSection);
            IsAnswerCorrect = isCorrect;
            
            if (isCorrect)
            {
                // Award XP for correct answers
                var xp = CalculateXPForSection(CurrentSection);
                XpEarned += xp;
                
                // Record progress
                var progressRecord = new ProgressRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = await _userService.GetCurrentUserIdAsync(),
                    LessonId = CurrentLesson?.Id ?? string.Empty,
                    SectionId = CurrentSection.Id,
                    Score = 1.0,
                    Accuracy = 1.0,
                    TimeSpentSeconds = 30, // Would be calculated from start/end times
                    XPEarned = xp,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                
                await _progressService.RecordProgressAsync(progressRecord);
            }
            
            // Move to next section or complete lesson
            if (CurrentSectionIndex < Sections.Count - 1)
            {
                await NavigateToSectionAsync(CurrentSectionIndex + 1);
            }
            else
            {
                await CompleteLessonAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error submitting answer: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task NavigateToNextSectionAsync()
    {
        if (CurrentSectionIndex < Sections.Count - 1)
        {
            await NavigateToSectionAsync(CurrentSectionIndex + 1);
        }
        else
        {
            await CompleteLessonAsync();
        }
    }
    
    private async Task CompleteLessonAsync()
    {
        try
        {
            IsLoading = true;
            var userId = await _userService.GetCurrentUserIdAsync();
            
            if (!string.IsNullOrEmpty(userId) && CurrentLesson != null)
            {
                var result = await _lessonService.CompleteLessonAsync(
                    userId, 
                    CurrentLesson.Id, 
                    XpEarned);
                    
                if (result.IsSuccess)
                {
                    // Navigate to completion screen
                    await Shell.Current.GoToAsync($"//lessonComplete?xp={XpEarned}&lessonId={CurrentLesson.Id}");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Failed to complete lesson";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error completing lesson: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private bool ValidateAnswer(string userAnswer, LessonSection section)
    {
        // Implementation would depend on section type
        // For vocabulary/translation sections, check against correct answers
        // For pronunciation sections, use speech recognition service
        return true; // Placeholder
    }
    
    private int CalculateXPForSection(LessonSection section)
    {
        // Different XP values based on section type and difficulty
        return section.Type switch
        {
            "vocabulary" => 10,
            "grammar" => 15,
            "pronunciation" => 20,
            "quiz" => 25,
            _ => 5
        };
    }
}
```

## UI Implementation with Horus Material Controls

### Lesson Player Page (XAML)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="LinguaLearn.Mobile.Views.LessonPlayerPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:material="clr-namespace:HorusStudio.Maui.MaterialDesignControls;assembly=HorusStudio.Maui.MaterialDesignControls"
             xmlns:viewmodels="clr-namespace:LinguaLearn.Mobile.ViewModels"
             x:DataType="viewmodels:LessonPlayerViewModel"
             Title="{Binding CurrentLesson.Title}"
             BackgroundColor="{AppThemeBinding Light={StaticResource BackgroundLight}, Dark={StaticResource BackgroundDark}}">
    
    <Grid RowDefinitions="Auto,*,Auto">
        <!-- Progress Bar -->
        <material:MaterialProgressIndicator Grid.Row="0"
                                           Progress="{Binding CurrentSectionIndex, Converter={StaticResource ProgressConverter}}"
                                           Maximum="{Binding Sections.Count}"
                                           ProgressColor="{StaticResource Primary}"
                                           BackgroundColor="{StaticResource SurfaceVariant}"
                                           HeightRequest="4"
                                           CornerRadius="2" />
        
        <!-- Lesson Content -->
        <ScrollView Grid.Row="1" Padding="20">
            <StackLayout Spacing="20">
                <!-- Section Header -->
                <StackLayout Orientation="Horizontal" Spacing="10">
                    <material:MaterialBadge Text="{Binding CurrentSection.Type}"
                                           BackgroundColor="{StaticResource Primary}"
                                           TextColor="{StaticResource OnPrimary}"
                                           CornerRadius="12"
                                           FontSize="12"
                                           HeightRequest="24" />
                    
                    <Label Text="{Binding CurrentSection.Title}"
                           FontSize="20"
                           FontAttributes="Bold"
                           TextColor="{AppThemeBinding Light={StaticResource OnBackgroundLight}, Dark={StaticResource OnBackgroundDark}}"
                           HorizontalOptions="FillAndExpand" />
                </StackLayout>
                
                <!-- Content Area -->
                <material:MaterialCard Elevation="2"
                                      CornerRadius="15"
                                      Padding="20"
                                      BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                    <StackLayout Spacing="15">
                        <!-- Text Content -->
                        <Label Text="{Binding CurrentSection.Content}"
                               FontSize="16"
                               TextColor="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}"
                               LineHeight="1.4" />
                        
                        <!-- Audio Player (for pronunciation sections) -->
                        <StackLayout IsVisible="{Binding CurrentSection.Type, Converter={StaticResource StringEqualsConverter}, ConverterParameter=pronunciation}"
                                   Spacing="10">
                            <Button Text="🔊 Play Pronunciation"
                                    Command="{Binding PlaySectionAudioCommand}"
                                    IsEnabled="{Binding IsPlayingAudio, Converter={StaticResource InverseBooleanConverter}}"
                                    BackgroundColor="{StaticResource Primary}"
                                    TextColor="{StaticResource OnPrimary}"
                                    CornerRadius="8" />
                            
                            <ActivityIndicator IsRunning="{Binding IsPlayingAudio}"
                                             IsVisible="{Binding IsPlayingAudio}"
                                             Color="{StaticResource Primary}" />
                        </StackLayout>
                        
                        <!-- Answer Input (for interactive sections) -->
                        <StackLayout IsVisible="{Binding CurrentSection.Type, Converter={StaticResource StringEqualsConverter}, ConverterParameter=vocabulary}"
                                   Spacing="10">
                            <material:MaterialTextField Label="Your Answer"
                                                      Text="{Binding UserAnswer}"
                                                      Placeholder="Type your answer here"
                                                      ReturnType="Done"
                                                      ReturnCommand="{Binding SubmitAnswerCommand}"
                                                      BackgroundColor="Transparent"
                                                      CornerRadius="8" />
                            
                            <!-- Answer Feedback -->
                            <StackLayout IsVisible="{Binding IsAnswerCorrect, Converter={StaticResource InverseBooleanConverter}}"
                                       Spacing="5">
                                <Label Text="❌ Incorrect"
                                       TextColor="{StaticResource Error}"
                                       FontSize="16"
                                       HorizontalOptions="Center" />
                                <Label Text="Please try again"
                                       TextColor="{StaticResource OnSurfaceVariant}"
                                       FontSize="14"
                                       HorizontalOptions="Center" />
                            </StackLayout>
                            
                            <StackLayout IsVisible="{Binding IsAnswerCorrect}"
                                       Spacing="5">
                                <Label Text="✅ Correct!"
                                       TextColor="{StaticResource Primary}"
                                       FontSize="16"
                                       HorizontalOptions="Center" />
                                <Label Text="{Binding XpEarned, StringFormat='+{0} XP'}"
                                       TextColor="{StaticResource Primary}"
                                       FontSize="14"
                                       HorizontalOptions="Center" />
                            </StackLayout>
                        </StackLayout>
                    </StackLayout>
                </material:MaterialCard>
            </StackLayout>
        </ScrollView>
        
        <!-- Action Buttons -->
        <StackLayout Grid.Row="2" 
                    Orientation="Horizontal" 
                    Padding="20,10"
                    Spacing="15">
            
            <!-- Previous Button -->
            <material:MaterialIconButton Icon="arrow_back"
                                        Command="{Binding NavigateToSectionCommand}"
                                        CommandParameter="{Binding CurrentSectionIndex, Converter={StaticResource SubtractConverter}}"
                                        IsEnabled="{Binding CurrentSectionIndex, Converter={StaticResource GreaterThanConverter}, ConverterParameter=0}"
                                        BackgroundColor="{StaticResource SurfaceVariant}"
                                        IconColor="{StaticResource OnSurfaceVariant}"
                                        HeightRequest="48"
                                        WidthRequest="48"
                                        CornerRadius="24" />
            
            <!-- Submit/Next Button -->
            <material:MaterialButton Text="{Binding CurrentSectionIndex, Converter={StaticResource NextButtonLabelConverter}, ConverterParameter={x:Static Binding.Sources}}"
                                   Command="{Binding CurrentSection.Type, Converter={StaticResource SectionActionCommandConverter}}"
                                   CommandParameter="{Binding UserAnswer}"
                                   IsEnabled="{Binding CurrentSection.Type, Converter={StaticResource RequiresAnswerConverter}}"
                                   BackgroundColor="{StaticResource Primary}"
                                   TextColor="{StaticResource OnPrimary}"
                                   HeightRequest="48"
                                   HorizontalOptions="FillAndExpand"
                                   CornerRadius="24" />
            
            <!-- Next Button -->
            <material:MaterialIconButton Icon="arrow_forward"
                                        Command="{Binding NavigateToNextSectionCommand}"
                                        IsEnabled="{Binding CurrentSectionIndex, Converter={StaticResource LessThanConverter}, ConverterParameter={Binding Sections.Count}}"
                                        BackgroundColor="{StaticResource SurfaceVariant}"
                                        IconColor="{StaticResource OnSurfaceVariant}"
                                        HeightRequest="48"
                                        WidthRequest="48"
                                        CornerRadius="24" />
        </StackLayout>
    </Grid>
</ContentPage>
```

### Quiz Page (XAML)
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="LinguaLearn.Mobile.Views.QuizPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:material="clr-namespace:HorusStudio.Maui.MaterialDesignControls;assembly=HorusStudio.Maui.MaterialDesignControls"
             xmlns:viewmodels="clr-namespace:LinguaLearn.Mobile.ViewModels"
             x:DataType="viewmodels:QuizViewModel"
             Title="{Binding CurrentQuiz.Title}"
             BackgroundColor="{AppThemeBinding Light={StaticResource BackgroundLight}, Dark={StaticResource BackgroundDark}}">
    
    <Grid RowDefinitions="Auto,Auto,*,Auto">
        <!-- Header with Timer and Progress -->
        <Grid Grid.Row="0" 
              ColumnDefinitions="Auto,*,Auto"
              Padding="20,10"
              ColumnSpacing="10">
            
            <!-- Timer -->
            <material:MaterialBadge Grid.Column="0"
                                   Text="{Binding TimeRemaining, StringFormat='{0:mm\\:ss}'}"
                                   BackgroundColor="{StaticResource Error}"
                                   TextColor="{StaticResource OnError}"
                                   CornerRadius="12"
                                   FontSize="12"
                                   HeightRequest="24" />
            
            <!-- Progress -->
            <StackLayout Grid.Column="1"
                        HorizontalOptions="Center"
                        VerticalOptions="Center">
                <Label Text="{Binding CurrentQuestionIndex, StringFormat='Question {0}'}"
                       TextColor="{AppThemeBinding Light={StaticResource OnBackgroundLight}, Dark={StaticResource OnBackgroundDark}}"
                       FontSize="16"
                       HorizontalOptions="Center" />
                <material:MaterialProgressIndicator Progress="{Binding CurrentQuestionIndex}"
                                                   Maximum="{Binding TotalQuestions}"
                                                   ProgressColor="{StaticResource Primary}"
                                                   BackgroundColor="{StaticResource SurfaceVariant}"
                                                   HeightRequest="4"
                                                   CornerRadius="2" />
            </StackLayout>
            
            <!-- XP Counter -->
            <material:MaterialBadge Grid.Column="2"
                                   Text="{Binding XpEarned, StringFormat='+{0} XP'}"
                                   BackgroundColor="{StaticResource Primary}"
                                   TextColor="{StaticResource OnPrimary}"
                                   CornerRadius="12"
                                   FontSize="12"
                                   HeightRequest="24" />
        </Grid>
        
        <!-- Question Content -->
        <ScrollView Grid.Row="1" Padding="20,0">
            <StackLayout Spacing="20">
                <!-- Question Text -->
                <material:MaterialCard Elevation="2"
                                      CornerRadius="15"
                                      Padding="20"
                                      BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                    <Label Text="{Binding CurrentQuestion.Question}"
                           FontSize="18"
                           FontAttributes="Bold"
                           TextColor="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}" />
                </material:MaterialCard>
                
                <!-- Answer Options (Multiple Choice) -->
                <StackLayout IsVisible="{Binding CurrentQuestion.Type, Converter={StaticResource StringEqualsConverter}, ConverterParameter=multiple_choice}"
                           Spacing="10">
                    <material:MaterialCard Elevation="1"
                                          CornerRadius="12"
                                          Padding="15"
                                          BackgroundColor="{Binding SelectedOption, Converter={StaticResource OptionBackgroundConverter}, ConverterParameter={x:Static Binding.Sources}}"
                                          Stroke="{Binding SelectedOption, Converter={StaticResource OptionBorderConverter}, ConverterParameter={x:Static Binding.Sources}}"
                                          StrokeThickness="2">
                        <StackLayout Orientation="Horizontal" Spacing="10">
                            <material:MaterialRadioButton IsChecked="{Binding SelectedOption, Converter={StaticResource OptionSelectedConverter}, ConverterParameter=0}"
                                                         Color="{StaticResource Primary}"
                                                         VerticalOptions="Center" />
                            <Label Text="{Binding CurrentQuestion.Options[0]}"
                                   FontSize="16"
                                   TextColor="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}"
                                   VerticalOptions="Center"
                                   HorizontalOptions="FillAndExpand" />
                        </StackLayout>
                        <material:MaterialCard.GestureRecognizers>
                            <TapGestureRecognizer Command="{Binding SelectOptionCommand}" CommandParameter="0" />
                        </material:MaterialCard.GestureRecognizers>
                    </material:MaterialCard>
                    
                    <material:MaterialCard Elevation="1"
                                          CornerRadius="12"
                                          Padding="15"
                                          BackgroundColor="{Binding SelectedOption, Converter={StaticResource OptionBackgroundConverter}, ConverterParameter=1}"
                                          Stroke="{Binding SelectedOption, Converter={StaticResource OptionBorderConverter}, ConverterParameter=1}"
                                          StrokeThickness="2">
                        <StackLayout Orientation="Horizontal" Spacing="10">
                            <material:MaterialRadioButton IsChecked="{Binding SelectedOption, Converter={StaticResource OptionSelectedConverter}, ConverterParameter=1}"
                                                         Color="{StaticResource Primary}"
                                                         VerticalOptions="Center" />
                            <Label Text="{Binding CurrentQuestion.Options[1]}"
                                   FontSize="16"
                                   TextColor="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}"
                                   VerticalOptions="Center"
                                   HorizontalOptions="FillAndExpand" />
                        </StackLayout>
                        <material:MaterialCard.GestureRecognizers>
                            <TapGestureRecognizer Command="{Binding SelectOptionCommand}" CommandParameter="1" />
                        </material:MaterialCard.GestureRecognizers>
                    </material:MaterialCard>
                    
                    <!-- Additional options would follow the same pattern -->
                </StackLayout>
                
                <!-- Text Input (Fill in the Blank) -->
                <material:MaterialTextField IsVisible="{Binding CurrentQuestion.Type, Converter={StaticResource StringEqualsConverter}, ConverterParameter=fill_blank}"
                                          Label="Your Answer"
                                          Text="{Binding UserAnswer}"
                                          Placeholder="Type your answer here"
                                          ReturnType="Done"
                                          ReturnCommand="{Binding SubmitAnswerCommand}"
                                          BackgroundColor="Transparent"
                                          CornerRadius="8" />
                
                <!-- Feedback -->
                <StackLayout IsVisible="{Binding ShowFeedback}"
                           Spacing="10">
                    <material:MaterialDivider />
                    
                    <StackLayout Orientation="Horizontal" 
                               HorizontalOptions="Center"
                               Spacing="10">
                        <Label Text="{Binding IsAnswerCorrect, Converter={StaticResource FeedbackIconConverter}}"
                               FontSize="24" />
                        <Label Text="{Binding IsAnswerCorrect, Converter={StaticResource FeedbackTextConverter}}"
                               FontSize="18"
                               FontAttributes="Bold"
                               VerticalOptions="Center" />
                    </StackLayout>
                    
                    <Label Text="{Binding CurrentQuestion.Explanation}"
                           FontSize="14"
                           TextColor="{AppThemeBinding Light={StaticResource OnSurfaceVariantLight}, Dark={StaticResource OnSurfaceVariantDark}}" />
                </StackLayout>
            </StackLayout>
        </ScrollView>
        
        <!-- Action Buttons -->
        <StackLayout Grid.Row="2" 
                    Padding="20"
                    VerticalOptions="End">
            <material:MaterialButton Text="{Binding ShowFeedback, Converter={StaticResource ActionButtonTextConverter}}"
                                   Command="{Binding ShowFeedback, Converter={StaticResource ActionButtonCommandConverter}}"
                                   BackgroundColor="{StaticResource Primary}"
                                   TextColor="{StaticResource OnPrimary}"
                                   HeightRequest="56"
                                   CornerRadius="28" />
        </StackLayout>
    </Grid>
</ContentPage>
```

## Performance Optimization

### Caching Strategy
```csharp
public class CachedLessonService : ILessonService
{
    private readonly ILessonService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedLessonService> _logger;
    
    public async Task<ServiceResult<Lesson?>> GetLessonAsync(string lessonId, CancellationToken ct = default)
    {
        var cacheKey = $"lesson_{lessonId}";
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out Lesson? cachedLesson))
        {
            _logger.LogDebug("Retrieved lesson {LessonId} from cache", lessonId);
            return ServiceResult<Lesson?>.Success(cachedLesson);
        }
        
        // If not in cache, get from service
        var result = await _innerService.GetLessonAsync(lessonId, ct);
        
        if (result.IsSuccess && result.Data != null)
        {
            // Cache for 1 hour
            _cache.Set(cacheKey, result.Data, TimeSpan.FromHours(1));
        }
        
        return result;
    }
}
```

### Pagination for Large Collections
```csharp
public async Task<ServiceResult<List<Lesson>>> GetLessonsPaginatedAsync(
    int page = 1, 
    int pageSize = 20, 
    CancellationToken ct = default)
{
    try
    {
        var query = _firestoreRepository.GetCollection("lessons")
            .OrderBy("order")
            .Limit(pageSize)
            .Offset((page - 1) * pageSize);
            
        var lessons = await _firestoreRepository.QueryCollectionAsync<Lesson>(query, ct);
        return ServiceResult<List<Lesson>>.Success(lessons);
    }
    catch (Exception ex)
    {
        return ServiceResult<List<Lesson>>.Failure($"Failed to get lessons: {ex.Message}");
    }
}
```

## Error Handling and Resilience

### Retry Policy with Polly
```csharp
public class ResilientLessonService : ILessonService
{
    private readonly ILessonService _innerService;
    private readonly IAsyncPolicy _retryPolicy;
    
    public ResilientLessonService(ILessonService innerService)
    {
        _innerService = innerService;
        
        _retryPolicy = Policy
            .Handle<FirestoreException>(ex => ex.ErrorCode == FirestoreErrorCode.Unavailable)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                });
    }
    
    public async Task<ServiceResult<Lesson?>> GetLessonAsync(string lessonId, CancellationToken ct = default)
    {
        return await _retryPolicy.ExecuteAsync(() => 
            _innerService.GetLessonAsync(lessonId, ct));
    }
}
```

## Testing Strategy

### Unit Tests
```csharp
public class LessonServiceTests
{
    [Fact]
    public async Task GetLessonAsync_WhenLessonExists_ReturnsLesson()
    {
        // Arrange
        var mockRepository = new Mock<IFirestoreRepository>();
        var mockAuthService = new Mock<IFirebaseAuthService>();
        var logger = new Mock<ILogger<LessonService>>();
        
        var lesson = new Lesson { Id = "test-lesson", Title = "Test Lesson" };
        mockRepository.Setup(r => r.GetDocumentAsync<Lesson>("lessons", "test-lesson", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<Lesson>.Success(lesson));
            
        var service = new LessonService(mockRepository.Object, mockAuthService.Object, logger.Object);
        
        // Act
        var result = await service.GetLessonAsync("test-lesson");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-lesson", result.Data?.Id);
        Assert.Equal("Test Lesson", result.Data?.Title);
    }
}
```

### Integration Tests
```csharp
public class LessonPlayerViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenValidLessonId_LoadsLesson()
    {
        // Arrange
        var mockLessonService = new Mock<ILessonService>();
        var mockProgressService = new Mock<IProgressService>();
        var mockUserService = new Mock<IUserService>();
        var mockAudioService = new Mock<IAudioService>();
        
        var lesson = new Lesson 
        { 
            Id = "test-lesson", 
            Title = "Test Lesson",
            Sections = new List<LessonSection>
            {
                new() { Id = "section-1", Title = "Section 1", Type = "vocabulary" }
            }
        };
        
        mockLessonService.Setup(s => s.GetLessonAsync("test-lesson", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<Lesson?>.Success(lesson));
            
        var viewModel = new LessonPlayerViewModel(
            mockLessonService.Object,
            mockProgressService.Object,
            mockUserService.Object,
            mockAudioService.Object);
            
        // Act
        await viewModel.InitializeAsync("test-lesson");
        
        // Assert
        Assert.Equal("Test Lesson", viewModel.CurrentLesson?.Title);
        Assert.Single(viewModel.Sections);
        Assert.Equal("Section 1", viewModel.CurrentSection?.Title);
    }
}
```

## Deployment and Monitoring

### Performance Metrics
- App Launch Time: < 2.5s (Android mid-range)
- Lesson Load Time: < 1.2s after auth
- Memory Usage: < 140MB (Android) typical
- Battery Usage: Minimize continuous microphone/animations

### Telemetry Events
- LessonStarted: { lessonId, difficulty, ts }
- LessonCompleted: { lessonId, xpEarned, accuracy, duration }
- QuestionAnswered: { questionId, isCorrect, responseTime }
- StreakExtended: { streakCount }
- BadgeEarned: { badgeId }

### Monitoring
- Crash Reporting: Firebase Crashlytics
- Performance Monitoring: Firebase Performance Monitoring
- Analytics: Firebase Analytics
- Logging: ILogger with structured logging

## Security Considerations

### Data Validation
- Validate all input data before Firestore operations
- Implement server-side validation rules
- Sanitize user inputs

### Authentication
- Always validate authentication before Firestore operations
- Implement automatic token refresh
- Handle authentication failures gracefully

### Security Rules
- Design Firestore security rules to match client operations
- Test security rules thoroughly
- Implement proper user authorization checks

## Future Enhancements

1. **Social Features**: Friend lists, direct challenges, collaborative learning
2. **AI-based Pronunciation**: Phoneme alignment service for advanced scoring
3. **Content Authoring**: Web admin dashboard for creating lessons
4. **Personalization**: ML model for difficulty hints and content recommendations
5. **Offline Mode**: Enhanced offline capabilities with sync queue
6. **Advanced Analytics**: Detailed learning insights and progress visualization

This implementation plan provides a comprehensive approach to building the main game logic for LinguaLearn with real-time Firestore integration, Material 3 design, and Horus Material controls for a modern, interactive UI.