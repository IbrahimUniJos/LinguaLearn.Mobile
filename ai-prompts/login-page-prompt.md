# AI Prompt for Login Page Generation

## App Context
Create a login page for a language learning app called LinguaLearn using .NET MAUI with MVVM pattern. The app uses Firebase Authentication for user authentication.

## Technology Requirements
- .NET MAUI with C# 12
- MVVM pattern using CommunityToolkit.Mvvm
- Use Horus MAUI Material Design Controls (MaterialTextField, MaterialButton, etc.)
- Firebase Authentication service integration

## User Model
The user has the following properties:
- UserId (string)
- Email (string)
- DisplayName (string?)
- IdToken (string)
- RefreshToken (string)
- ExpiresAt (DateTime)

## Authentication Service Interface
The app uses IFirebaseAuthService with these methods:
- Task<ServiceResult<UserSession>> SignInWithEmailAsync(string email, string password, CancellationToken ct = default)
- Task SignOutAsync()
- Task<UserSession?> GetCurrentSessionAsync()
- bool IsAuthenticated { get; }

## UI Requirements

### Controls to Use
1. MaterialTextField for email input
2. MaterialTextField for password input (with IsPassword=true)
3. MaterialButton for login button
4. MaterialButton for navigation to signup page
5. MaterialActivityIndicator for loading state during authentication
6. MaterialSnackbar for error messages

### Layout Requirements
- Clean, modern design following Material Design 3 guidelines
- App logo at the top
- "Welcome Back" heading
- Email and Password fields with proper validation
- "Login" primary button
- "Don't have an account? Sign Up" link button
- "Forgot Password?" link (optional)
- Loading indicator when authentication is in progress
- Proper error handling and user feedback

### Workflow Requirements
1. User opens the login page
2. User enters email and password
3. User clicks "Login" button
4. Form validates inputs locally:
   - Email format validation
   - Password length validation (minimum 6 characters)
5. If validation passes:
   - Show loading indicator
   - Call AuthService.SignInWithEmailAsync()
6. If authentication succeeds:
   - Navigate to main app (lessons page)
7. If authentication fails:
   - Hide loading indicator
   - Show error message in snackbar
8. "Sign Up" button navigates to registration page

### Validation Rules
- Email: Required, valid email format
- Password: Required, minimum 6 characters

### Error Handling
- Network errors
- Invalid credentials
- Account not found
- Account disabled
- Too many requests

## Files to Generate
1. Views/Auth/LoginPage.xaml (UI layout)
2. Views/Auth/LoginPage.xaml.cs (Code-behind)
3. ViewModels/AuthViewModel.cs (Shared view model for auth operations)
4. Update AppShell.xaml to register the route

## Specific Implementation Details
- Use MaterialTextField for input fields with proper styling
- Implement INotifyDataErrorInfo for validation in ViewModel
- Use RelayCommand from CommunityToolkit.Mvvm for commands
- Handle loading states with IsBusy property
- Use WeakReferenceMessenger for navigation events if needed
- Follow MVVM best practices (no business logic in code-behind)
- Use proper binding for all UI elements
- Implement proper accessibility attributes
- Support dark/light theme switching

## Example Structure
```
<ContentPage>
    <ScrollView>
        <VerticalStackLayout>
            <Image Source="app_logo.png" />
            <Label Text="Welcome Back" />
            <MaterialTextField x:Name="EmailEntry" Text="{Binding Email}" />
            <MaterialTextField x:Name="PasswordEntry" Text="{Binding Password}" IsPassword="True" />
            <MaterialButton Text="Login" Command="{Binding LoginCommand}" IsEnabled="{Binding IsBusy, Converter={InvertedBoolConverter}}" />
            <MaterialActivityIndicator IsRunning="{Binding IsBusy}" IsVisible="{Binding IsBusy}" />
            <MaterialButton Text="Sign Up" Command="{Binding NavigateToSignUpCommand}" Style="{StaticResource TextButtonStyle}" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

## ViewModel Requirements
- Observable properties for Email, Password, IsBusy
- RelayCommand for LoginCommand
- RelayCommand for NavigateToSignUpCommand
- Validation logic for email and password
- Error handling and messaging
- Navigation logic after successful authentication