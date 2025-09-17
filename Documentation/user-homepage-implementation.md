# User Homepage Implementation Plan

## Overview
This document outlines the implementation plan for the user homepage of the LinguaLearn mobile application. The homepage will serve as the central hub for users, displaying their profile information, gamification stats, progress tracking, and quick access to key features.

## Design Principles
- Modern, sleek Material Design 3 interface
- Responsive layout for all device sizes
- Intuitive user experience with clear visual hierarchy
- Consistent with existing app branding and navigation
- Efficient data loading and real-time updates

## Technology Stack
- **Framework**: .NET MAUI
- **UI Controls**: Horus Material Design Controls
- **Data Source**: Google Cloud Firestore
- **State Management**: MVVM with CommunityToolkit.Mvvm
- **Navigation**: .NET MAUI Shell

## Homepage Components

### 1. User Profile Header
Displays core user information at the top of the page:
- User avatar or initials
- Display name
- Current level with XP progress indicator
- Streak counter with fire icon

### 2. Daily Goal Progress
Visual representation of weekly learning goals:
- Circular progress chart showing weekly progress
- Current streak display
- Weekly goal completion status

### 3. Quick Actions
Large touch targets for primary user actions:
- Continue Learning
- Daily Challenge
- Practice Pronunciation
- Review Vocabulary

### 4. Recent Activity
Card-based list of recent user activities:
- Completed lessons
- Earned badges
- Quiz results
- Streak milestones

### 5. Leaderboard Preview
Preview of user's position in the weekly leaderboard:
- User's rank
- Points comparison with nearby users
- Link to full leaderboard

## Data Models

### UserProfile (from existing models)
```csharp
public class UserProfile
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string? NativeLanguage { get; set; }
    public string? TargetLanguage { get; set; }
    
    // Gamification Stats
    public int XP { get; set; }
    public int Level { get; set; }
    public int StreakCount { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public int StreakFreezeTokens { get; set; }
    
    // Preferences
    public UserPreferences Preferences { get; set; }
    
    // Badges
    public List<UserBadge> Badges { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### UserStats (aggregated data)
```csharp
public class UserStats
{
    public string UserId { get; set; }
    public int TotalLessonsCompleted { get; set; }
    public int TotalQuizzesCompleted { get; set; }
    public int TotalXPEarned { get; set; }
    public double AverageQuizAccuracy { get; set; }
    public int TotalStudyTimeMinutes { get; set; }
    public int LongestStreak { get; set; }
    public int BadgesEarned { get; set; }
    public WeeklyProgress CurrentWeek { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

## ViewModel Implementation

### UserHomepageViewModel
```csharp
public partial class UserHomepageViewModel : ObservableObject
{
    private readonly IFirestoreRepository _firestoreRepository;
    private readonly IUserPreferencesService _preferencesService;
    private readonly IAuthenticationService _authService;
    
    [ObservableProperty]
    private UserProfile? _userProfile;
    
    [ObservableProperty]
    private UserStats? _userStats;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public UserHomepageViewModel(
        IFirestoreRepository firestoreRepository,
        IUserPreferencesService preferencesService,
        IAuthenticationService authService)
    {
        _firestoreRepository = firestoreRepository;
        _preferencesService = preferencesService;
        _authService = authService;
    }
    
    public async Task InitializeAsync()
    {
        await LoadUserDataAsync();
    }
    
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadUserDataAsync();
    }
    
    [RelayCommand]
    private async Task NavigateToLessonsAsync()
    {
        await Shell.Current.GoToAsync("//lessons");
    }
    
    [RelayCommand]
    private async Task NavigateToLeaderboardAsync()
    {
        await Shell.Current.GoToAsync("//leaderboard");
    }
    
    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await Shell.Current.GoToAsync("//profile");
    }
    
    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            var userId = _authService.CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "User not authenticated";
                return;
            }
            
            // Load user profile
            var profileResult = await _firestoreRepository.GetDocumentAsync<UserProfile>("users", userId);
            if (profileResult.IsSuccess)
            {
                UserProfile = profileResult.Data;
            }
            else
            {
                ErrorMessage = "Failed to load user profile";
                return;
            }
            
            // Load user stats
            var statsResult = await _firestoreRepository.GetDocumentAsync<UserStats>("userStats", userId);
            if (statsResult.IsSuccess)
            {
                UserStats = statsResult.Data;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## View Implementation (XAML)

### UserHomepagePage.xaml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="LinguaLearn.Mobile.Views.UserHomepagePage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:material="clr-namespace:HorusStudio.Maui.MaterialDesignControls;assembly=HorusStudio.Maui.MaterialDesignControls"
             xmlns:viewmodels="clr-namespace:LinguaLearn.Mobile.ViewModels"
             x:DataType="viewmodels:UserHomepageViewModel"
             Title="Home"
             BackgroundColor="{AppThemeBinding Light={StaticResource BackgroundLight}, Dark={StaticResource BackgroundDark}}">
    
    <ContentPage.Resources>
        <ResourceDictionary>
            <Style x:Key="HeaderLabelStyle" TargetType="Label">
                <Setter Property="FontFamily" Value="OpenSansSemibold" />
                <Setter Property="FontSize" Value="24" />
                <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource OnBackgroundLight}, Dark={StaticResource OnBackgroundDark}}" />
            </Style>
            
            <Style x:Key="SubHeaderLabelStyle" TargetType="Label">
                <Setter Property="FontFamily" Value="OpenSansRegular" />
                <Setter Property="FontSize" Value="18" />
                <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}" />
            </Style>
            
            <Style x:Key="BodyLabelStyle" TargetType="Label">
                <Setter Property="FontFamily" Value="OpenSansRegular" />
                <Setter Property="FontSize" Value="16" />
                <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource OnSurfaceLight}, Dark={StaticResource OnSurfaceDark}}" />
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>
    
    <RefreshView IsRefreshing="{Binding IsLoading}"
                 Command="{Binding RefreshDataCommand}">
        <ScrollView>
            <StackLayout Spacing="20" Padding="20">
                
                <!-- User Profile Header -->
                <material:MaterialCard Elevation="4" 
                                       CornerRadius="20" 
                                       Padding="20"
                                       BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                    <Grid ColumnDefinitions="Auto,*,Auto" 
                          RowDefinitions="Auto,Auto" 
                          ColumnSpacing="15">
                        
                        <!-- User Avatar -->
                        <Border Grid.Row="0" 
                                Grid.Column="0"
                                Grid.RowSpan="2"
                                StrokeShape="RoundRectangle 30"
                                WidthRequest="60"
                                HeightRequest="60"
                                BackgroundColor="{StaticResource Primary}">
                            <Label Text="{Binding UserProfile.DisplayName[0]}"
                                   HorizontalOptions="Center"
                                   VerticalOptions="Center"
                                   FontSize="24"
                                   TextColor="{StaticResource OnPrimary}"
                                   FontAttributes="Bold" />
                        </Border>
                        
                        <!-- User Info -->
                        <Label Grid.Row="0" 
                               Grid.Column="1"
                               Text="{Binding UserProfile.DisplayName}"
                               Style="{StaticResource HeaderLabelStyle}" />
                        
                        <StackLayout Grid.Row="1" 
                                     Grid.Column="1"
                                     Orientation="Horizontal"
                                     Spacing="10">
                            <Label Text="Level"
                                   Style="{StaticResource BodyLabelStyle}" />
                            <Label Text="{Binding UserProfile.Level}"
                                   Style="{StaticResource BodyLabelStyle}"
                                   FontAttributes="Bold"
                                   TextColor="{StaticResource Primary}" />
                            
                            <material:MaterialDivider Orientation="Vertical"
                                                      WidthRequest="1"
                                                      HeightRequest="20"
                                                      Color="{StaticResource Outline}" />
                            
                            <Label Text="XP"
                                   Style="{StaticResource BodyLabelStyle}" />
                            <Label Text="{Binding UserProfile.XP}"
                                   Style="{StaticResource BodyLabelStyle}"
                                   FontAttributes="Bold"
                                   TextColor="{StaticResource Primary}" />
                        </StackLayout>
                        
                        <!-- Streak Counter -->
                        <StackLayout Grid.Row="0"
                                     Grid.Column="2"
                                     Grid.RowSpan="2"
                                     Orientation="Horizontal"
                                     Spacing="5"
                                     HorizontalOptions="End"
                                     VerticalOptions="Center">
                            <Label Text="🔥"
                                   FontSize="24" />
                            <Label Text="{Binding UserProfile.StreakCount}"
                                   Style="{StaticResource HeaderLabelStyle}"
                                   VerticalOptions="Center" />
                        </StackLayout>
                    </Grid>
                </material:MaterialCard>
                
                <!-- Daily Goal Progress -->
                <material:MaterialCard Elevation="4" 
                                       CornerRadius="20" 
                                       Padding="20"
                                       BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                    <StackLayout Spacing="15">
                        <Label Text="Weekly Goal"
                               Style="{StaticResource HeaderLabelStyle}" />
                        
                        <Grid ColumnDefinitions="*,Auto" 
                              RowDefinitions="Auto,Auto">
                            
                            <!-- Progress Ring -->
                            <Grid Grid.Row="0"
                                  Grid.Column="0"
                                  Grid.RowSpan="2"
                                  WidthRequest="120"
                                  HeightRequest="120"
                                  HorizontalOptions="Start">
                                <BoxView CornerRadius="60"
                                         Color="{StaticResource SurfaceVariant}" />
                                <Path Data="M 60,60 L 60,0 A 60,60 0 1,1 30,15 Z"
                                      Stroke="{StaticResource Primary}"
                                      StrokeThickness="8"
                                      HorizontalOptions="Center"
                                      VerticalOptions="Center"
                                      Aspect="Uniform" />
                            </Grid>
                            
                            <!-- Goal Info -->
                            <Label Grid.Row="0"
                                   Grid.Column="1"
                                   Text="{Binding UserStats.CurrentWeek.LessonsCompleted, StringFormat='{0} lessons'}"
                                   Style="{StaticResource SubHeaderLabelStyle}" />
                            
                            <Label Grid.Row="1"
                                   Grid.Column="1"
                                   Text="{Binding UserStats.CurrentWeek.Goal, StringFormat='Goal: {0} lessons'}"
                                   Style="{StaticResource BodyLabelStyle}" />
                        </Grid>
                    </StackLayout>
                </material:MaterialCard>
                
                <!-- Quick Actions -->
                <StackLayout Spacing="15">
                    <Label Text="Quick Actions"
                           Style="{StaticResource HeaderLabelStyle}" />
                    
                    <Grid ColumnDefinitions="*,*" 
                          RowDefinitions="Auto,Auto"
                          ColumnSpacing="15"
                          RowSpacing="15">
                        
                        <material:MaterialCard Grid.Row="0"
                                               Grid.Column="0"
                                               Elevation="2"
                                               CornerRadius="15"
                                               Padding="15"
                                               BackgroundColor="{StaticResource Primary}">
                            <StackLayout Spacing="10">
                                <Label Text="📚"
                                       FontSize="32"
                                       HorizontalOptions="Center" />
                                <Label Text="Continue Learning"
                                       Style="{StaticResource BodyLabelStyle}"
                                       TextColor="{StaticResource OnPrimary}"
                                       HorizontalOptions="Center"
                                       HorizontalTextAlignment="Center" />
                            </StackLayout>
                            <material:MaterialCard.GestureRecognizers>
                                <TapGestureRecognizer Command="{Binding NavigateToLessonsCommand}" />
                            </material:MaterialCard.GestureRecognizers>
                        </material:MaterialCard>
                        
                        <material:MaterialCard Grid.Row="0"
                                               Grid.Column="1"
                                               Elevation="2"
                                               CornerRadius="15"
                                               Padding="15"
                                               BackgroundColor="{StaticResource Secondary}">
                            <StackLayout Spacing="10">
                                <Label Text="🏆"
                                       FontSize="32"
                                       HorizontalOptions="Center" />
                                <Label Text="Daily Challenge"
                                       Style="{StaticResource BodyLabelStyle}"
                                       TextColor="{StaticResource OnSecondary}"
                                       HorizontalOptions="Center"
                                       HorizontalTextAlignment="Center" />
                            </StackLayout>
                        </material:MaterialCard>
                        
                        <material:MaterialCard Grid.Row="1"
                                               Grid.Column="0"
                                               Elevation="2"
                                               CornerRadius="15"
                                               Padding="15"
                                               BackgroundColor="{StaticResource Tertiary}">
                            <StackLayout Spacing="10">
                                <Label Text="🎤"
                                       FontSize="32"
                                       HorizontalOptions="Center" />
                                <Label Text="Practice Pronunciation"
                                       Style="{StaticResource BodyLabelStyle}"
                                       TextColor="{StaticResource OnTertiary}"
                                       HorizontalOptions="Center"
                                       HorizontalTextAlignment="Center" />
                            </StackLayout>
                        </material:MaterialCard>
                        
                        <material:MaterialCard Grid.Row="1"
                                               Grid.Column="1"
                                               Elevation="2"
                                               CornerRadius="15"
                                               Padding="15"
                                               BackgroundColor="{StaticResource Primary}">
                            <StackLayout Spacing="10">
                                <Label Text="📖"
                                       FontSize="32"
                                       HorizontalOptions="Center" />
                                <Label Text="Review Vocabulary"
                                       Style="{StaticResource BodyLabelStyle}"
                                       TextColor="{StaticResource OnPrimary}"
                                       HorizontalOptions="Center"
                                       HorizontalTextAlignment="Center" />
                            </StackLayout>
                        </material:MaterialCard>
                    </Grid>
                </StackLayout>
                
                <!-- Recent Activity -->
                <StackLayout Spacing="15">
                    <StackLayout Orientation="Horizontal"
                                 HorizontalOptions="FillAndExpand">
                        <Label Text="Recent Activity"
                               Style="{StaticResource HeaderLabelStyle}"
                               HorizontalOptions="Start" />
                        <material:MaterialIconButton Icon="chevron_right"
                                                     HorizontalOptions="EndAndExpand"
                                                     Command="{Binding NavigateToProfileCommand}" />
                    </StackLayout>
                    
                    <StackLayout BindableLayout.ItemsSource="{Binding RecentActivities}">
                        <BindableLayout.ItemTemplate>
                            <DataTemplate x:DataType="models:ActivityItem">
                                <material:MaterialCard Elevation="2"
                                                       CornerRadius="15"
                                                       Padding="15"
                                                       Margin="0,0,0,10"
                                                       BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                                    <Grid ColumnDefinitions="Auto,*" 
                                          ColumnSpacing="15">
                                        <Label Grid.Column="0"
                                               Text="{Binding Icon}"
                                               FontSize="24" />
                                        <StackLayout Grid.Column="1"
                                                     Spacing="5">
                                            <Label Text="{Binding Title}"
                                                   Style="{StaticResource SubHeaderLabelStyle}" />
                                            <Label Text="{Binding Description}"
                                                   Style="{StaticResource BodyLabelStyle}"
                                                   FontSize="14" />
                                            <Label Text="{Binding Timestamp, StringFormat='{0:MMM dd, yyyy}'}"
                                                   Style="{StaticResource BodyLabelStyle}"
                                                   FontSize="12"
                                                   TextColor="{StaticResource Outline}" />
                                        </StackLayout>
                                    </Grid>
                                </material:MaterialCard>
                            </DataTemplate>
                        </BindableLayout.ItemTemplate>
                    </StackLayout>
                </StackLayout>
                
                <!-- Leaderboard Preview -->
                <material:MaterialCard Elevation="4" 
                                       CornerRadius="20" 
                                       Padding="20"
                                       BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceLight}, Dark={StaticResource SurfaceDark}}">
                    <StackLayout Spacing="15">
                        <StackLayout Orientation="Horizontal">
                            <Label Text="Leaderboard"
                                   Style="{StaticResource HeaderLabelStyle}"
                                   HorizontalOptions="Start" />
                            <material:MaterialIconButton Icon="chevron_right"
                                                         HorizontalOptions="EndAndExpand"
                                                         Command="{Binding NavigateToLeaderboardCommand}" />
                        </StackLayout>
                        
                        <StackLayout Orientation="Horizontal"
                                     Spacing="10">
                            <Label Text="Your rank:"
                                   Style="{StaticResource BodyLabelStyle}" />
                            <Label Text="{Binding UserRank}"
                                   Style="{StaticResource BodyLabelStyle}"
                                   FontAttributes="Bold"
                                   TextColor="{StaticResource Primary}" />
                        </StackLayout>
                        
                        <ProgressBar Progress="{Binding RankProgress}"
                                     ProgressColor="{StaticResource Primary}"
                                     HeightRequest="10"
                                     CornerRadius="5" />
                    </StackLayout>
                </material:MaterialCard>
            </StackLayout>
        </ScrollView>
    </RefreshView>
</ContentPage>
```

## Firestore Integration

### Data Access Layer
The homepage will utilize the existing `IFirestoreRepository` service for data access:

```csharp
// In MauiProgram.cs, register the view model and view
builder.Services.AddTransient<UserHomepageViewModel>();
builder.Services.AddTransient<UserHomepagePage>();
```

### Collection Structure
Data will be retrieved from the following Firestore collections:
1. `users/{userId}` - User profile information
2. `userStats/{userId}` - Aggregated user statistics
3. `activities/{userId}/recent` - Recent user activities (subcollection)
4. `leaderboards/weekly` - Weekly leaderboard data

## Performance Considerations

1. **Data Caching**: Implement local caching for user profile and stats to reduce Firestore calls
2. **Incremental Loading**: Load recent activities in paginated chunks
3. **Real-time Updates**: Use Firestore listeners for streak count and XP updates
4. **Image Optimization**: Use efficient image loading for avatars and badges
5. **Loading States**: Show skeleton loaders during data fetching

## Accessibility Features

1. **Semantic Labels**: All interactive elements have descriptive automation names
2. **Screen Reader Support**: Proper heading levels and content descriptions
3. **Color Contrast**: Sufficient contrast ratios for text and UI elements
4. **Keyboard Navigation**: Support for keyboard-only navigation
5. **Dynamic Text Scaling**: Responsive to system font size settings

## Error Handling

1. **Network Failures**: Display appropriate error messages for connectivity issues
2. **Data Loading Errors**: Show retry options when data fails to load
3. **Authentication Errors**: Redirect to login if user session expires
4. **Graceful Degradation**: Show available data even if some components fail

## Testing Strategy

1. **Unit Tests**: ViewModel logic and data transformation
2. **Integration Tests**: Firestore data access and caching layer
3. **UI Tests**: Navigation flows and user interactions
4. **Performance Tests**: Load times and memory usage
5. **Accessibility Tests**: Screen reader compatibility and keyboard navigation

## Future Enhancements

1. **Personalized Recommendations**: AI-driven lesson suggestions
2. **Social Features**: Friend activity feeds and challenges
3. **Achievement Showcase**: Interactive badge display
4. **Learning Insights**: Detailed progress analytics
5. **Offline Mode**: Cached homepage for offline access